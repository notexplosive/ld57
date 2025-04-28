using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExplogineCore;
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
    private readonly List<PlacedEntity> _moveBuffer = new();
    private readonly List<UiElement> _uiElements = new();
    private GridPosition _cameraPosition;
    private UiElement? _currentPopup;
    private EditorTool _editorTool;
    private string? _fileName;
    private GridPosition? _hoveredTilePosition;
    private bool _isDraggingPrimary;
    private bool _isDraggingSecondary;
    private GridPosition? _moveStart;
    private AsciiScreen _screen;
    private EntityTemplate? _selectedTemplate;
    private GridPosition? _selectionAnchor;
    private Rectangle? _selectionRectangle;
    private bool _shouldClosePopup;

    public EditorSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = RebuildScreenWithWidth(50);
        _cameraPosition = DefaultCameraPosition();

        WorldTemplate = new WorldTemplate();

        if (HotReloadCache.EditorOpenFileName != null)
        {
            var data = Client.Debug.RepoFileSystem.ReadFile(
                $"Resource/Worlds/{HotReloadCache.EditorOpenFileName}.json");
            var template = JsonConvert.DeserializeObject<WorldTemplate>(data);
            if (template != null)
            {
                SetTemplate(HotReloadCache.EditorOpenFileName, template);
            }
        }

        if (HotReloadCache.EditorCameraPosition.HasValue)
        {
            _cameraPosition = HotReloadCache.EditorCameraPosition.Value;
        }
    }

    private static GridPosition DefaultCameraPosition()
    {
        return new GridPosition(-3, -4);
    }

    public WorldTemplate WorldTemplate { get; private set; }

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
        int j = 0;
        
        var tempWorld = new World(new GridPosition(1,1), new WorldTemplate(), true);
        foreach (var template in LdResourceAssets.Instance.EntityTemplates.Values)
        {
            var tempEntity = new Entity(tempWorld, new GridPosition(0, 0), template);
            tilePalette.AddSelectable(new SelectableButton(new GridPosition(1 + i, 1 + j),
                tempEntity.TileState,
                selectedTemplate, () => { _selectedTemplate = template; }));
            
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

        if (_editorTool == EditorTool.Write)
        {
            if (HoveredTileWorldPosition.HasValue)
            {
                var metadataEntities = WorldTemplate.GetMetadataAt(HoveredTileWorldPosition.Value).ToList();

                if (metadataEntities.Count > 1)
                {
                    for (int i = 1; i < metadataEntities.Count; i++)
                    {
                        Client.Debug.Log("Removed duplicate metadata");
                        WorldTemplate.RemoveExactEntity(metadataEntities[i]);
                    }
                }
                
                if (metadataEntities.Count > 0 &&  metadataEntities.First().ExtraState.TryGetValue(Constants.CommandKey, out var status))
                {
                    return $"{Constants.CommandKey}: " + status;
                }
            }
        }

        return string.Empty;
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.F5).WasPressed)
            {
                var player = WorldTemplate.GetPlayerEntity();
                var position = new GridPosition();
                if (player != null)
                {
                    position = player.Position;
                }
                RequestPlay?.Invoke(position);
            }
        }
        
        HotReloadCache.EditorOpenFileName = _fileName;
        HotReloadCache.EditorCameraPosition = _cameraPosition;

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

        if (input.Keyboard.Modifiers.Control && input.Keyboard.GetButton(Keys.O, true).WasPressed)
        {
            Open();
        }

        if (input.Keyboard.Modifiers.Control && input.Keyboard.GetButton(Keys.N, true).WasPressed)
        {
            Save();
            SetTemplate(null, new WorldTemplate());
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
                        WorldTemplate.RemoveEntitiesAtExceptMetadata(HoveredTileWorldPosition.Value);
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
                if (_moveStart != null && HoveredTileWorldPosition.HasValue && _selectionRectangle != null)
                {
                    var offset = HoveredTileWorldPosition.Value - _moveStart.Value;
                    _selectionRectangle = _selectionRectangle.Value.Moved(offset.ToPoint());

                    foreach (var item in _moveBuffer)
                    {
                        item.Position += offset;
                    }

                    _moveStart = HoveredTileWorldPosition.Value;
                }

                break;
        }

        _hoveredTilePosition = _screen.GetHoveredTile(input, hitTestStack, Vector2.Zero);
    }

    private void Open()
    {
        var fullPath =
            PlatformFileApi.OpenFileDialogue("Open World", new PlatformFileApi.ExtensionDescription("json", "JSON"));
        if (!string.IsNullOrEmpty(fullPath))
        {
            var fileName = new FileInfo(fullPath).Name;

            var json = Client.Debug.RepoFileSystem.ReadFile(fullPath);
            var newWorld = JsonConvert.DeserializeObject<WorldTemplate>(json);

            if (newWorld != null)
            {
                Client.Debug.RepoFileSystem.WriteToFile($"Resource/Worlds/{fileName}", json);
                var newFileName = fileName.RemoveFileExtension();
                SetTemplate(newFileName, newWorld);
            }
        }
    }

    private void SetTemplate(string? newFileName, WorldTemplate newWorld)
    {
        _fileName = newFileName;
        WorldTemplate = newWorld;
        _cameraPosition = DefaultCameraPosition();
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
                    _moveStart = position;
                    break;
                case EditorTool.Fill:
                    DoFillAt(position);
                    break;
                case EditorTool.Write:
                    var foundMetaEntity = WorldTemplate.GetMetadataAt(position).FirstOrDefault();
                    var defaultText = "";
                    if (foundMetaEntity != null)
                    {
                        defaultText = foundMetaEntity.ExtraState.GetValueOrDefault(Constants.CommandKey) ?? defaultText;
                    }

                    var isUsingSelection = _selectionRectangle.HasValue &&
                                           Constants.ContainsInclusive(_selectionRectangle.Value, position);
                    RequestText("Enter Command", defaultText,
                        text =>
                        {
                            if (foundMetaEntity != null)
                            {
                                if (string.IsNullOrEmpty(text))
                                {
                                    WorldTemplate.RemoveExactEntity(foundMetaEntity);
                                }
                                else
                                {
                                    foundMetaEntity.ExtraState[Constants.CommandKey] = text;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(text))
                                {
                                    if (isUsingSelection)
                                    {
                                        foreach (var cell in Constants.AllPositionsInRectangle(_selectionRectangle!
                                                     .Value))
                                        {
                                            WorldTemplate.AddMetaEntity(cell, text);
                                        }
                                    }
                                    else
                                    {
                                        WorldTemplate.AddMetaEntity(position, text);
                                    }
                                }
                            }
                        });
                    break;
                case EditorTool.Play:
                    if (_fileName != null)
                    {
                        Save();
                        RequestPlay?.Invoke(position);
                    }
                    else
                    {
                        Save();
                        Client.Debug.Log("Name the level first!");
                    }

                    break;
                case EditorTool.Connect:
                    IncrementSignalAt(position, 1);
                    break;
            }
        }

        if (mouseButton == MouseButton.Right)
        {
            if (_editorTool == EditorTool.Connect)
            {
                IncrementSignalAt(position, -1);
            }

            _isDraggingSecondary = true;
        }
    }

    private void IncrementSignalAt(GridPosition position, int delta)
    {
        foreach (var entity in WorldTemplate.AllEntitiesAt(position))
        {
            var templateName = entity.TemplateName;
            if (string.IsNullOrEmpty(templateName))
            {
                return;
            }

            var template = ResourceAlias.EntityTemplate(templateName);
            if (template.Tags.Contains("Signal"))
            {
                if (entity.ExtraState.TryGetValue("channel", out var result))
                {
                    var newValue = int.Parse(result) + delta;
                    SetChannelValue(entity, newValue);
                }
                else
                {
                    SetChannelValue(entity, 0 + delta);
                }
            }
        }
    }

    private static void SetChannelValue(PlacedEntity entity, int newValue)
    {
        if (!LdResourceAssets.Instance.HasNamedColor($"signal_{newValue}"))
        {
            return;
        }

        entity.ExtraState["channel"] = newValue.ToString();
    }

    private void RequestText(string message, string? defaultText, Action<string> onSubmit)
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
                    foreach (var item in _moveBuffer)
                    {
                        WorldTemplate.RemoveEntitiesAt(item.Position);
                    }

                    foreach (var item in _moveBuffer)
                    {
                        WorldTemplate.AddExactEntity(item);
                    }

                    _moveStart = null;
                    break;
            }
        }

        if (mouseButton == MouseButton.Right)
        {
            _isDraggingSecondary = false;
        }
    }

    private void ClearSelection()
    {
        _selectionRectangle = null;
        _moveBuffer.Clear();
    }

    private void CreateSelection(GridPosition position)
    {
        if (!_selectionAnchor.HasValue)
        {
            return;
        }

        _selectionRectangle = RectangleF.FromCorners(position.ToPoint().ToVector2(),
            _selectionAnchor.Value.ToPoint().ToVector2()).ToRectangle();

        _moveBuffer.Clear();
        foreach (var grabbedPosition in Constants.AllPositionsInRectangle(
                     new GridPosition(_selectionRectangle.Value.Location),
                     new GridPosition(_selectionRectangle.Value.Location +
                                      _selectionRectangle.Value.Size)))
        {
            _moveBuffer.AddRange(WorldTemplate.AllEntitiesAt(grabbedPosition));
        }

        _selectionAnchor = null;
    }

    public override void Update(float dt)
    {
        _screen.Clear(TileState.TransparentEmpty);

        var world = new World(Constants.GameRoomSize, WorldTemplate, true);

        var player = WorldTemplate.GetPlayerEntity();
        if (player != null)
        {
            world.AddEntity(new Entity(world, player.Position, ResourceAlias.EntityTemplate("player")));
        }

        world.SetCurrentRoom(new Room(world, _cameraPosition, _cameraPosition + _screen.RoomSize));
        world.CameraPosition = _cameraPosition;

        world.PaintToScreen(_screen, dt);

        if (_hoveredTilePosition.HasValue)
        {
            var originalTile = _screen.GetTile(_hoveredTilePosition.Value);

            var uiElement = HitUiElement(_hoveredTilePosition.Value);
            if (uiElement == null)
            {
                if (_editorTool == EditorTool.Brush || _editorTool == EditorTool.Connect)
                {
                    var tile = originalTile with {BackgroundColor = Color.LightBlue, BackgroundIntensity = 0.75f};
                    _screen.PutTile(_hoveredTilePosition.Value, tile);
                }

                if (_editorTool == EditorTool.Write)
                {
                    var tile = TileState.StringCharacter("!");
                    _screen.PutTile(_hoveredTilePosition.Value, tile);
                }

                if (_editorTool == EditorTool.Play)
                {
                    var tile = TileState.Sprite(ResourceAlias.Entities, 0);
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
            var hoveredRoomTopLeft = hoveredRoom.TopLeft - _cameraPosition;
            var hoveredRoomBottomRight = hoveredRoom.BottomRight - _cameraPosition;
            
            var width = (hoveredRoomBottomRight.X - hoveredRoomTopLeft.X);
            var height = (hoveredRoomBottomRight.Y - hoveredRoomTopLeft.Y);
            
            foreach (var position in Constants.TraceRectangle(hoveredRoomTopLeft, hoveredRoomBottomRight))
            {
                var color = Color.LightBlue;
                var previousTileState = _screen.GetTile(position);
                var increment = 2;

                var normalX = (position.X - hoveredRoomTopLeft.X);
                var normalY = (position.Y - hoveredRoomTopLeft.Y);
                
                if (normalX % increment == 0 && (normalY == 0 || normalY == height))
                {
                    color = Color.Lime;
                }

                if (normalY % increment == 0 && (normalX == 0 || normalX == width))
                {
                    color = Color.ForestGreen;
                }
                
                var midX = hoveredRoomTopLeft.X + width / 2;
                if (position.X == midX || position.X == midX + 1)
                {
                    color = Color.Orange;
                }

                var midY = hoveredRoomTopLeft.Y + height / 2;

                if (position.Y == midY || position.Y == midY + 1)
                {
                    color = Color.Orange;
                }

                _screen.PutTile(position, previousTileState with {BackgroundColor = color, BackgroundIntensity = 1f});
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
                    previousTileState with {BackgroundColor = Color.Goldenrod, ForegroundColor = Color.DarkGoldenrod, BackgroundIntensity = 1f});
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
                    previousTileState with {BackgroundColor = Color.LimeGreen, ForegroundColor = Color.Green, BackgroundIntensity = 1f});
            }
        }

        if (MathF.Sin(Client.TotalElapsedTime * 10) > 0)
        {
            foreach (var placedEntity in WorldTemplate.PlacedEntities)
            {
                if (placedEntity.ExtraState.ContainsKey(Constants.CommandKey))
                {
                    _screen.PutTile(placedEntity.Position - _cameraPosition,
                        TileState.StringCharacter("!", Color.OrangeRed));
                }

                if (placedEntity.ExtraState.ContainsKey("channel"))
                {
                    /*
                    _screen.PutTile(placedEntity.Position - _cameraPosition,
                        TileState.Sprite(ResourceAlias.Tools, 4, ResourceAlias.Color("signal_"+ placedEntity.ExtraData["channel"])));
                        */
                }
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
            CloseCurrentPopup();
        };
    }

    private void CloseCurrentPopup()
    {
        _shouldClosePopup = true;
    }
}
