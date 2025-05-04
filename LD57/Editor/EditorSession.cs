using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using LD57.CartridgeManagement;
using LD57.Core;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class EditorSession : Session
{
    private readonly List<IEditorTool> _editorTools = new();
    private readonly EditorSelector<EntityTemplate> _templateSelector = new();
    private readonly EditorSelector<IEditorTool> _toolSelector = new();
    private readonly List<UiElement> _uiElements = new();
    private GridPosition _cameraPosition;
    private UiElement? _currentPopup;
    private GridPosition? _hoveredScreenPosition;
    private AsciiScreen _screen;
    private bool _shouldClosePopup;

    public EditorSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _editorTools.Add(new BrushTool(this));
        _editorTools.Add(new SelectionTool(this));
        _editorTools.Add(new ChangeSignalTool(this));
        _editorTools.Add(new TriggerTool(this));
        _editorTools.Add(new PlayTool(this));

        _screen = RebuildScreenWithWidth(46);
        Surface = new WorldEditorSurface(this);
        _cameraPosition = DefaultCameraPosition();

        var cachedFileName = HotReloadCache.LevelEditorOpenFileName;
        if (cachedFileName != null)
        {
            Surface.Open(cachedFileName, false);
        }

        if (HotReloadCache.LevelEditorCameraPosition.HasValue)
        {
            _cameraPosition = HotReloadCache.LevelEditorCameraPosition.Value;
        }
    }

    public WorldEditorSurface Surface { get; }
    public WorldSelection WorldSelection { get; } = new();
    public GridPosition? MoveStart { get; set; }
    public bool IsDraggingSecondary { get; private set; }
    public bool IsDraggingPrimary { get; private set; }

    public GridPosition? HoveredWorldPosition
    {
        get
        {
            if (_hoveredScreenPosition.HasValue)
            {
                if (HitUiElement(_hoveredScreenPosition.Value) != null)
                {
                    return null;
                }
            }

            return _cameraPosition + _hoveredScreenPosition;
        }
    }

    private IEditorTool? CurrentTool => _toolSelector.Selected;
    public EntityTemplate? SelectedTemplate => _templateSelector.Selected;

    private static GridPosition DefaultCameraPosition()
    {
        return new GridPosition(-3, -4);
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

        var leftToolbar = new UiElement(new GridPosition(0, 0), new GridPosition(2, _editorTools.Count + 1));

        var toolIndex = 0;
        foreach (var tool in _editorTools)
        {
            leftToolbar.AddSelectable(new SelectableButton<IEditorTool>(new GridPosition(1, 1 + toolIndex),
                tool.TileStateInToolbar, _toolSelector, tool));
            toolIndex++;
        }

        _uiElements.Add(leftToolbar);

        var statusBar = new UiElement(bottomLeftCorner, new GridPosition(screen.Width - 1, screen.Height - 1));
        statusBar.AddDynamicText(new GridPosition(4, 0), () =>
        {
            if (!HoveredWorldPosition.HasValue)
            {
                return string.Empty;
            }

            return $"M({HoveredWorldPosition.Value.X:D3},{HoveredWorldPosition.Value.Y:D3})";
        });

        statusBar.AddDynamicText(new GridPosition(1, 1), Status);
        _uiElements.Add(statusBar);

        var tilePalette = new UiElement(new GridPosition(3, 0), new GridPosition(screen.Width - 1, 3));
        var i = 0;
        var j = 0;

        var tempWorld = new World(new GridPosition(1, 1), new WorldTemplate(), true);
        foreach (var template in LdResourceAssets.Instance.EntityTemplates.Values)
        {
            var tempEntity = new Entity(tempWorld, new GridPosition(0, 0), template);
            tilePalette.AddSelectable(new SelectableButton<EntityTemplate>(
                new GridPosition(1 + i, 1 + j), tempEntity.TileState,
                _templateSelector, template));

            i++;
            if (i > tilePalette.Width - 3)
            {
                i = 0;
                j++;
            }
        }

        _uiElements.Add(tilePalette);

        return screen;
    }

    private string Status()
    {
        return CurrentTool?.Status() ?? string.Empty;
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        HotReloadCache.LevelEditorOpenFileName = Surface.FileName;
        HotReloadCache.LevelEditorCameraPosition = _cameraPosition;

        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            if (_hoveredScreenPosition.HasValue)
            {
                var hitUiElement = HitUiElement(_hoveredScreenPosition.Value);
                if (hitUiElement != null && hitUiElement.Contains(_hoveredScreenPosition.Value))
                {
                    var subElement = hitUiElement.GetSubElementAt(_hoveredScreenPosition.Value);
                    subElement?.OnClicked();
                }
            }

            if (HoveredWorldPosition.HasValue)
            {
                StartMousePressInWorld(HoveredWorldPosition.Value, MouseButton.Left);
            }
        }

        if (input.Mouse.GetButton(MouseButton.Right).WasPressed)
        {
            if (HoveredWorldPosition.HasValue)
            {
                StartMousePressInWorld(HoveredWorldPosition.Value, MouseButton.Right);
            }
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            FinishMousePressInWorld(HoveredWorldPosition, MouseButton.Left);
        }

        if (input.Mouse.GetButton(MouseButton.Right).WasReleased)
        {
            FinishMousePressInWorld(HoveredWorldPosition, MouseButton.Right);
        }

        foreach (var element in _uiElements)
        {
            element.UpdateKeyboardInput(input.Keyboard);
        }

        if (_shouldClosePopup)
        {
            _currentPopup = null;
            _shouldClosePopup = false;
        }

        if (_currentPopup != null)
        {
            var enteredCharacters = input.Keyboard.GetEnteredCharacters();
            _currentPopup.OnTextInput(enteredCharacters);
            _currentPopup.UpdateKeyboardInput(input.Keyboard);
            return;
        }

        Surface.HandleKeyBinds(input);

        if (input.Keyboard.GetButton(Keys.S).WasPressed)
        {
            if (input.Keyboard.Modifiers.Control)
            {
                input.Keyboard.Consume(Keys.S);
                SaveFlow();
            }

            if (input.Keyboard.Modifiers.ControlAlt)
            {
                input.Keyboard.Consume(Keys.S);
                SaveAs();
            }
        }

        if (input.Keyboard.Modifiers.Control && input.Keyboard.GetButton(Keys.O, true).WasPressed)
        {
            OpenFlow();
        }

        if (input.Keyboard.Modifiers.Control && input.Keyboard.GetButton(Keys.N, true).WasPressed)
        {
            SaveFlow();
            Surface.Clear();
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
            _cameraPosition += new GridPosition(-_screen.Width / 4, 0);
        }

        if (input.Keyboard.GetButton(Keys.D).WasPressed)
        {
            _cameraPosition += new GridPosition(_screen.Width / 4, 0);
        }

        if (input.Keyboard.GetButton(Keys.W).WasPressed)
        {
            _cameraPosition += new GridPosition(0, -_screen.Height / 4);
        }

        if (input.Keyboard.GetButton(Keys.S).WasPressed)
        {
            _cameraPosition += new GridPosition(0, _screen.Height / 4);
        }

        CurrentTool?.UpdateInput(input.Keyboard);

        _hoveredScreenPosition = _screen.GetHoveredTile(input, hitTestStack, Vector2.Zero);

        if (!IsDraggingPrimary && !IsDraggingSecondary)
        {
            var scrollVector = new Vector2(0, input.Mouse.ScrollDelta());
            if (scrollVector.Y != 0)
            {
                var scrollDelta = (int) scrollVector.Normalized().Y;
                var delta = -scrollDelta;

                if (input.Keyboard.Modifiers.Control)
                {
                    if (CurrentTool != null)
                    {
                        var currentIndex = _editorTools.IndexOf(CurrentTool);
                        var newIndex = Math.Clamp(currentIndex + delta, 0, _editorTools.Count - 1);
                        _toolSelector.Selected = _editorTools[newIndex];
                    }
                }

                if (input.Keyboard.Modifiers.Shift)
                {
                    if (SelectedTemplate != null)
                    {
                        var allTemplates = LdResourceAssets.Instance.EntityTemplates.Values.ToList();
                        var currentIndex = allTemplates.IndexOf(SelectedTemplate);
                        var newIndex = Math.Clamp(currentIndex + delta, 0, allTemplates.Count - 1);
                        _templateSelector.Selected = allTemplates[newIndex];
                    }
                }
            }
        }
    }

    private void OpenFlow()
    {
        var fullPath =
            PlatformFileApi.OpenFileDialogue("Open World", new PlatformFileApi.ExtensionDescription("json", "JSON"));
        if (!string.IsNullOrEmpty(fullPath))
        {
            Surface.Open(fullPath, true);
        }
    }

    public event Action<GridPosition>? RequestPlay;

    private void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            IsDraggingPrimary = true;
        }

        if (mouseButton == MouseButton.Right)
        {
            IsDraggingSecondary = true;
        }

        CurrentTool?.StartMousePressInWorld(position, mouseButton);
    }

    public void RequestText(string message, string? defaultText, Action<string> onSubmit)
    {
        var topLeft = new GridPosition(4, 12);
        var textModal = new UiElement(topLeft, new GridPosition(_screen.Width - topLeft.X, topLeft.Y + 3));
        textModal.AddStaticText(new GridPosition(1, 1), message);
        var textInput = textModal.AddTextInput(new GridPosition(1, 2), defaultText ?? string.Empty);

        _currentPopup = textModal;
        textInput.Submitted += text =>
        {
            onSubmit(text);
            CloseCurrentPopup();
        };

        textInput.Cancelled += CloseCurrentPopup;
    }

    private void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            IsDraggingPrimary = false;
        }

        if (mouseButton == MouseButton.Right)
        {
            IsDraggingSecondary = false;
        }

        CurrentTool?.FinishMousePressInWorld(position, mouseButton);
    }

    public override void Update(float dt)
    {
        _screen.Clear(TileState.TransparentEmpty);

        Surface.PaintWorldToScreen(_screen, _cameraPosition, dt);

        if (_hoveredScreenPosition.HasValue)
        {
            var originalTile = _screen.GetTile(_hoveredScreenPosition.Value);

            var uiElement = HitUiElement(_hoveredScreenPosition.Value);
            if (uiElement == null)
            {
                var tile = CurrentTool?.GetTileStateInWorldOnHover(originalTile);
                if (tile.HasValue)
                {
                    _screen.PutTile(_hoveredScreenPosition.Value, tile.Value);
                }
            }
            else
            {
                var hoveredSubElement = uiElement.GetSubElementAt(_hoveredScreenPosition.Value);
                if (hoveredSubElement != null)
                {
                    hoveredSubElement.ShowHover(_screen, _hoveredScreenPosition.Value);
                }
            }
        }

        Surface.PaintWorldOverlayToScreen(_screen, _cameraPosition);

        foreach (var worldPosition in WorldSelection.AllPositions())
        {
            var screenPosition = worldPosition - _cameraPosition;
            if (_screen.ContainsPosition(screenPosition))
            {
                _screen.PutTile(screenPosition, WorldSelection.GetTileState(worldPosition - WorldSelection.Offset));
            }
        }

        CurrentTool?.PaintToScreen(_screen, _cameraPosition);

        Surface.DrawWorldOverlay(_screen, _cameraPosition);

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

    public void SaveFlow()
    {
        if (Surface.FileName == null)
        {
            SaveAs();
            return;
        }

        Surface.Save(Surface.FileName);
    }

    private void SaveAs()
    {
        var topLeft = new GridPosition(4, 12);
        var saveAsPopup = new UiElement(topLeft, new GridPosition(_screen.Width - topLeft.X, topLeft.Y + 3));
        saveAsPopup.AddStaticText(new GridPosition(1, 1), "Name this World:");
        var textInput = saveAsPopup.AddTextInput(new GridPosition(1, 2), Surface.FileName);
        _currentPopup = saveAsPopup;
        textInput.Submitted += text =>
        {
            Surface.FileName = text;
            SaveFlow();
            CloseCurrentPopup();
        };
    }

    private void CloseCurrentPopup()
    {
        _shouldClosePopup = true;
    }

    public void RequestPlayAt(GridPosition position)
    {
        RequestPlay?.Invoke(position);
    }

    public void ResetCameraPosition()
    {
        _cameraPosition = DefaultCameraPosition();
    }
}
