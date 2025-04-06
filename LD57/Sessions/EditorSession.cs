﻿using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using LD57.CartridgeManagement;
using LD57.Editor;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace LD57.Sessions;

public class EditorSession : Session
{
    private readonly List<UiElement> _uiElements = new();
    private GridPosition _cameraPosition;
    private UiElement? _currentPopup;
    private EditorTool _editorTool;
    private string? _fileName;
    private GridPosition? _hoveredTilePosition;
    private bool _isDraggingPrimary;
    private bool _isDraggingSecondary;
    private AsciiScreen _screen;
    private EntityTemplate? _selectedTemplate;
    private GridPosition? _selectionAnchor;
    private Rectangle? _selectionRectangle;

    public EditorSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = RebuildScreenWithWidth(50);
        _cameraPosition += new GridPosition(-3, -4);

        _fileName = "default";
        var template = JsonConvert.DeserializeObject<WorldTemplate>(Client.Debug.RepoFileSystem.GetDirectory("Resource/Worlds")
            .ReadFile(_fileName + ".json"));
        
        WorldTemplate = template ?? new WorldTemplate();
    }

    public WorldTemplate WorldTemplate { get; }

    public GridPosition? HoveredTileWorldPosition
    {
        get
        {
            if (_hoveredTilePosition.HasValue)
            {
                if (HitUiElement(_hoveredTilePosition.Value) != null)
                {
                    return null;
                }
            }

            return _cameraPosition + _hoveredTilePosition;
        }
    }

    private AsciiScreen RebuildScreenWithWidth(int width)
    {
        if (width < 10)
        {
            return _screen;
        }

        var tileSize = 1920 / width;
        var height = 1080 / tileSize;

        _uiElements.Clear();

        var screen = new AsciiScreen(width, height, tileSize);

        var bottomLeftCorner = new GridPosition(0, screen.Height - 3);
        var editorTools = Enum.GetValues<EditorTool>();

        var leftToolbar = new UiElement(new GridPosition(0, 0), new GridPosition(2, editorTools.Length + 1));

        var selectedTool = new EditorSelector();

        foreach (var enumValue in editorTools)
        {
            leftToolbar.AddSelectable(new SelectableButton(new GridPosition(1, 1 + (int) enumValue),
                TileState.Sprite(ResourceAlias.Tools, (int) enumValue),
                selectedTool, () => { _editorTool = enumValue; }));
        }

        _uiElements.Add(leftToolbar);

        var statusBar = new UiElement(bottomLeftCorner, new GridPosition(screen.Width - 1, screen.Height - 1));
        statusBar.AddDynamicText(new GridPosition(4, 0), () =>
        {
            if (!HoveredTileWorldPosition.HasValue)
            {
                return string.Empty;
            }

            return $"M({HoveredTileWorldPosition.Value.X:D3},{HoveredTileWorldPosition.Value.Y:D3})";
        });

        statusBar.AddDynamicText(new GridPosition(1, 1), Status);
        _uiElements.Add(statusBar);

        var tilePalette = new UiElement(new GridPosition(3, 0), new GridPosition(screen.Width - 1, 3));
        var selectedTemplate = new EditorSelector();
        var i = 0;
        foreach (var template in LdResourceAssets.Instance.EntityTemplates.Values)
        {
            var tempEntity = new Entity(new GridPosition(0, 0), template);
            tilePalette.AddSelectable(new SelectableButton(new GridPosition(1 + i, 1), tempEntity.TileState,
                selectedTemplate, () => { _selectedTemplate = template; }));
            i++;
        }

        _uiElements.Add(tilePalette);

        return screen;
    }

    private string Status()
    {
        if (_editorTool == EditorTool.Select && _selectionRectangle != null)
        {
            return "[F]ill";
        }

        if (_editorTool == EditorTool.Brush)
        {
            return "Pencil";
        }
        
        if (_editorTool == EditorTool.Play)
        {
            return "Play from Select Location";
        }

        return string.Empty;
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            if (_hoveredTilePosition.HasValue)
            {
                var hitUiElement = HitUiElement(_hoveredTilePosition.Value);
                if (hitUiElement != null && hitUiElement.Contains(_hoveredTilePosition.Value))
                {
                    var subElement = hitUiElement.GetSubElementAt(_hoveredTilePosition.Value);
                    subElement?.OnClicked();
                }
            }

            if (HoveredTileWorldPosition.HasValue)
            {
                StartMousePressInWorld(HoveredTileWorldPosition.Value, MouseButton.Left);
            }
        }

        if (input.Mouse.GetButton(MouseButton.Right).WasPressed)
        {
            if (HoveredTileWorldPosition.HasValue)
            {
                StartMousePressInWorld(HoveredTileWorldPosition.Value, MouseButton.Right);
            }
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            FinishMousePressInWorld(HoveredTileWorldPosition, MouseButton.Left);
        }

        if (input.Mouse.GetButton(MouseButton.Right).WasReleased)
        {
            FinishMousePressInWorld(HoveredTileWorldPosition, MouseButton.Right);
        }

        foreach (var element in _uiElements)
        {
            element.UpdateKeyboardInput(input.Keyboard);
        }

        if (_currentPopup != null)
        {
            var enteredCharacters = input.Keyboard.GetEnteredCharacters();
            _currentPopup.UpdateKeyboardInput(input.Keyboard);
            _currentPopup.OnTextInput(enteredCharacters);
            return;
        }

        if (input.Keyboard.GetButton(Keys.S).WasPressed)
        {
            if (input.Keyboard.Modifiers.Control)
            {
                input.Keyboard.Consume(Keys.S);
                Save();
            }

            if (input.Keyboard.Modifiers.ControlAlt)
            {
                input.Keyboard.Consume(Keys.S);
                SaveAs();
            }
        }

        if (input.Keyboard.GetButton(Keys.OemMinus).WasPressed)
        {
            _screen = RebuildScreenWithWidth(_screen.Width + 1);
        }

        if (input.Keyboard.GetButton(Keys.OemPlus).WasPressed)
        {
            _screen = RebuildScreenWithWidth(_screen.Width - 1);
        }

        if (input.Keyboard.GetButton(Keys.A).WasPressed)
        {
            _cameraPosition = _cameraPosition + new GridPosition(-_screen.Width / 4, 0);
        }

        if (input.Keyboard.GetButton(Keys.D).WasPressed)
        {
            _cameraPosition = _cameraPosition + new GridPosition(_screen.Width / 4, 0);
        }

        if (input.Keyboard.GetButton(Keys.W).WasPressed)
        {
            _cameraPosition = _cameraPosition + new GridPosition(0, -_screen.Height / 4);
        }

        if (input.Keyboard.GetButton(Keys.S).WasPressed)
        {
            _cameraPosition = _cameraPosition + new GridPosition(0, _screen.Height / 4);
        }

        switch (_editorTool)
        {
            case EditorTool.Brush:
                if (HoveredTileWorldPosition.HasValue && _selectedTemplate != null)
                {
                    if (_isDraggingPrimary)
                    {
                        WorldTemplate.SetTile(HoveredTileWorldPosition.Value, _selectedTemplate);
                    }

                    if (_isDraggingSecondary)
                    {
                        WorldTemplate.RemoveEntitiesAt(HoveredTileWorldPosition.Value);
                    }
                }

                break;
            case EditorTool.Select:
                if (_selectionRectangle != null && _selectedTemplate != null)
                {
                    if (input.Keyboard.GetButton(Keys.F).WasPressed)
                    {
                        WorldTemplate.FillRectangle(_selectionRectangle.Value, _selectedTemplate);
                    }

                    if (input.Keyboard.GetButton(Keys.Delete).WasPressed)
                    {
                        WorldTemplate.EraseRectangle(_selectionRectangle.Value);
                    }
                }

                break;
            case EditorTool.Move:
                break;
            case EditorTool.Connect:
                break;
        }

        _hoveredTilePosition = _screen.GetHoveredTile(input, hitTestStack, Vector2.Zero);
    }

    public event Action<GridPosition>? RequestPlay;

    private void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            _isDraggingPrimary = true;

            switch (_editorTool)
            {
                case EditorTool.Select:
                    _selectionAnchor = position;
                    break;
                case EditorTool.Move:
                    break;
                case EditorTool.Fill:
                    DoFillAt(position);
                    break;
                case EditorTool.Write:
                    break;
                case EditorTool.Play:
                    if (_fileName != null)
                    {
                        Save();
                        RequestPlay?.Invoke(position);
                    }
                    else
                    {
                        Client.Debug.Log("Name the level first!");
                    }
                    break;
            }
        }

        if (mouseButton == MouseButton.Right)
        {
            _isDraggingSecondary = true;
        }
    }

    private void DoFillAt(GridPosition position)
    {
        Client.Debug.Log("Not implemented");
    }

    private void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            _isDraggingPrimary = false;

            switch (_editorTool)
            {
                case EditorTool.Select:
                    if (position.HasValue)
                    {
                        CreateSelection(position.Value);
                    }

                    break;
                case EditorTool.Move:
                    break;
            }
        }

        if (mouseButton == MouseButton.Right)
        {
            _isDraggingSecondary = false;
        }
    }

    private void CreateSelection(GridPosition position)
    {
        if (!_selectionAnchor.HasValue)
        {
            return;
        }

        if (position == _selectionAnchor.Value)
        {
            _selectionRectangle = null;
        }
        else
        {
            _selectionRectangle = RectangleF.FromCorners(position.ToPoint().ToVector2(),
                _selectionAnchor.Value.ToPoint().ToVector2()).ToRectangle();
        }

        _selectionAnchor = null;
    }

    public override void Update(float dt)
    {
        _screen.Clear(TileState.Empty);

        var world = new World(Constants.GameRoomSize, WorldTemplate);

        var player = WorldTemplate.GetPlayerEntity();
        if (player != null)
        {
            world.AddEntity(new Entity(player.Position, ResourceAlias.EntityTemplate("player")));
        }

        world.SetCurrentRoom(new Room(world, _cameraPosition, _cameraPosition + _screen.RoomSize));
        world.CameraPosition = _cameraPosition;

        world.PopulateOnScreen(_screen, dt);

        if (_hoveredTilePosition.HasValue)
        {
            var originalTile = _screen.GetTile(_hoveredTilePosition.Value);

            var uiElement = HitUiElement(_hoveredTilePosition.Value);
            if (uiElement == null)
            {
                if (_editorTool == EditorTool.Brush || _editorTool == EditorTool.Connect || _editorTool == EditorTool.Play)
                {
                    var tile = originalTile with {BackgroundColor = Color.LightBlue};
                    _screen.PutTile(_hoveredTilePosition.Value, tile);
                }
            }
            else
            {
                var hoveredSubElement = uiElement.GetSubElementAt(_hoveredTilePosition.Value);
                if (hoveredSubElement != null)
                {
                    hoveredSubElement.ShowHover(_screen, _hoveredTilePosition.Value);
                }
            }
        }

        if (HoveredTileWorldPosition.HasValue)
        {
            var hoveredRoom = world.GetRoomAt(HoveredTileWorldPosition.Value);
            foreach (var position in Constants.TraceRectangle(hoveredRoom.TopLeftPosition - _cameraPosition,
                         hoveredRoom.BottomRightPosition - _cameraPosition))
            {
                var previousTileState = _screen.GetTile(position);
                _screen.PutTile(position, previousTileState with {BackgroundColor = Color.LightBlue});
            }
        }

        if (_selectionRectangle.HasValue)
        {
            var topLeft = new GridPosition(_selectionRectangle.Value.Location) - _cameraPosition;
            var bottomRight = new GridPosition(_selectionRectangle.Value.Location + _selectionRectangle.Value.Size) -
                              _cameraPosition;

            foreach (var position in Constants.AllPositionsInRectangle(topLeft, bottomRight))
            {
                var previousTileState = _screen.GetTile(position);
                _screen.PutTile(position,
                    previousTileState with {BackgroundColor = Color.Goldenrod, ForegroundColor = Color.DarkGoldenrod});
            }
        }

        if (_selectionAnchor.HasValue && HoveredTileWorldPosition.HasValue && _isDraggingPrimary)
        {
            var topLeft = _selectionAnchor.Value - _cameraPosition;
            var bottomRight = HoveredTileWorldPosition.Value - _cameraPosition;

            foreach (var position in Constants.AllPositionsInRectangle(topLeft, bottomRight))
            {
                var previousTileState = _screen.GetTile(position);
                _screen.PutTile(position,
                    previousTileState with {BackgroundColor = Color.LimeGreen, ForegroundColor = Color.Green});
            }
        }

        foreach (var uiElement in _uiElements)
        {
            uiElement.PaintToScreen(_screen);
        }

        if (_currentPopup != null)
        {
            _currentPopup.PaintToScreen(_screen);
        }
    }

    private UiElement? HitUiElement(GridPosition hoveredTilePosition)
    {
        if (_currentPopup != null)
        {
            return _currentPopup;
        }

        for (var i = _uiElements.Count - 1; i >= 0; i--)
        {
            var ui = _uiElements[i];
            if (ui.Contains(hoveredTilePosition))
            {
                return _uiElements[i];
            }
        }

        return null;
    }

    public override void Draw(Painter painter)
    {
        _screen.Draw(painter, Vector2.Zero);
    }

    public void Save()
    {
        if (_fileName == null)
        {
            SaveAs();
            return;
        }

        Client.Debug.RepoFileSystem.WriteToFile($"Resource/Worlds/{_fileName}.json",
            JsonConvert.SerializeObject(WorldTemplate, Formatting.Indented));
    }

    private void SaveAs()
    {
        var topLeft = new GridPosition(4, 12);
        var saveAsPopup = new UiElement(topLeft, new GridPosition(_screen.Width - topLeft.X, topLeft.Y + 3));
        saveAsPopup.AddStaticText(new GridPosition(1, 1), "Name this World:");
        var textInput = saveAsPopup.AddTextInput(new GridPosition(1, 2), _fileName);
        _currentPopup = saveAsPopup;
        textInput.Submitted += text =>
        {
            _fileName = text;
            Save();
            _currentPopup = null;
        };
    }
}
