using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using LD57.Core;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class EditorSession : Session
{
    private readonly List<KeybindChord> _chords;
    private readonly List<IEditorTool> _editorTools = new();
    private readonly Stack<Popup> _popupStack = new();
    private readonly EditorSelector<IEditorTool> _toolSelector = new();
    private readonly List<UiElement> _uiElements = new();
    private GridPosition _cameraPosition;
    private Popup? _currentPopup;
    private GridPosition? _hoveredScreenPosition;
    private ISubElement? _primedElement;
    private AsciiScreen _screen;

    public EditorSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem, IEditorSurface surface,
        List<KeybindChord> chords) : base(
        runtimeWindow,
        runtimeFileSystem)
    {
        _chords = chords;
        _screen = RebuildScreenWithWidth(46);
        Surface = surface;
        Surface.RequestedResetCamera += ResetCameraPosition;
        Surface.RequestedPopup += OnPopupRequested;
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

    public GridRectangle CurrentScreenSize => _screen.RoomRectangle;
    public List<Func<AsciiScreen, UiElement>> ExtraUi { get; } = new();

    public IEditorSurface Surface { get; }
    public GridPosition? MoveStart { get; set; }
    public bool IsDraggingSecondary { get; private set; }
    public bool IsDraggingPrimary { get; private set; }

    public GridPosition? HoveredWorldPosition
    {
        get
        {
            if (_hoveredScreenPosition.HasValue)
            {
                if (UiElementAt(_hoveredScreenPosition.Value) != null)
                {
                    return null;
                }
            }

            return _cameraPosition + _hoveredScreenPosition;
        }
    }

    private IEditorTool? CurrentTool => _toolSelector.Selected;
    public List<Action<ConsumableInput, int>> ExtraKeyBinds { get; } = new();

    private void OnPopupRequested(CreatePopupDelegate popupDelegate)
    {
        var popup = popupDelegate(_screen.RoomRectangle);
        OpenPopup(popup);
    }

    public void RebuildScreen()
    {
        _screen = RebuildScreenWithWidth(_screen.Width);
    }

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

        var leftToolbar =
            new UiElement(new GridRectangle(new GridPosition(0, 0), new GridPosition(2, _editorTools.Count + 1)));

        var toolIndex = 0;
        foreach (var tool in _editorTools)
        {
            leftToolbar.AddSelectable(new SelectableButton<IEditorTool>(new GridPosition(1, 1 + toolIndex),
                tool.TileStateInToolbar, _toolSelector, tool));
            toolIndex++;
        }

        _uiElements.Add(leftToolbar);

        var statusBar =
            new UiElement(new GridRectangle(bottomLeftCorner, new GridPosition(screen.RoomSize.X, screen.RoomSize.Y)));
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

        foreach (var extraUi in ExtraUi)
        {
            _uiElements.Add(extraUi(screen));
        }

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

        if (input.Keyboard.GetButton(Keys.R).WasPressed)
        {
            RebuildScreen();
        }

        if (_hoveredScreenPosition.HasValue)
        {
            var hitUiElement = UiElementAt(_hoveredScreenPosition.Value);
            if (hitUiElement != null)
            {
                hitUiElement.UpdateMouseInput(input, _hoveredScreenPosition.Value, ref _primedElement);
            }
        }

        if (input.Mouse.GetButton(MouseButton.Middle).WasPressed)
        {
            if (HoveredWorldPosition.HasValue)
            {
                Surface.OnMiddleClickInWorld(HoveredWorldPosition.Value);
            }
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
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

        if (_currentPopup != null && _currentPopup.ShouldClose)
        {
            FinishClosingPopup();
        }

        _hoveredScreenPosition = _screen.GetHoveredTile(input, hitTestStack, Vector2.Zero);

        if (_currentPopup != null)
        {
            _currentPopup.UpdateKeyboardInput(input.Keyboard);

            var enteredCharacters = input.Keyboard.GetEnteredCharacters(true);
            _currentPopup.OnTextInput(enteredCharacters);
            return;
        }

        CurrentTool?.UpdateInput(input.Keyboard, HoveredWorldPosition);

        foreach (var chord in _chords)
        {
            chord.ListenForFirstKey(input, Surface);
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
            Surface.ClearEverything();
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
            MoveCameraKeybind(input.Keyboard.Modifiers,Direction.Left);
        }

        if (input.Keyboard.GetButton(Keys.D).WasPressed)
        {
            MoveCameraKeybind(input.Keyboard.Modifiers,Direction.Right);
        }

        if (input.Keyboard.GetButton(Keys.W).WasPressed)
        {
            MoveCameraKeybind(input.Keyboard.Modifiers,Direction.Up);
        }

        if (input.Keyboard.GetButton(Keys.S).WasPressed)
        {
            MoveCameraKeybind(input.Keyboard.Modifiers, Direction.Down);
        }

        if (!IsDraggingPrimary && !IsDraggingSecondary)
        {
            var flippedDelta = -input.Mouse.NormalizedScrollDelta();

            if (input.Keyboard.Modifiers.Control)
            {
                if (CurrentTool != null)
                {
                    var currentIndex = _editorTools.IndexOf(CurrentTool);
                    var newIndex = Math.Clamp(currentIndex + flippedDelta, 0, _editorTools.Count - 1);
                    _toolSelector.Selected = _editorTools[newIndex];
                }
            }

            foreach (var extraKeyBindEvent in ExtraKeyBinds)
            {
                extraKeyBindEvent(input, flippedDelta);
            }
        }
    }

    private void MoveCameraKeybind(ModifierKeys keyboardModifiers, Direction direction)
    {
        var fraction = 4;
        var stepSize = direction
            .ToVector()
            .StraightMultiply(_screen.Width  *  1f / fraction, _screen.Height * 1f / fraction)
            .RoundToGridPosition();

        if (keyboardModifiers.Shift)
        {
            stepSize = stepSize.ToVector2().Normalized().RoundToGridPosition();
        }
        
        _cameraPosition += stepSize;
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
        var textModal =
            new Popup(new GridRectangle(topLeft, new GridPosition(_screen.Width - topLeft.X, topLeft.Y + 1)));
        textModal.AddStaticText(new GridPosition(1, 1), message);
        var textInput = textModal.AddTextInput(new GridPosition(1, 2), defaultText ?? string.Empty);

        OpenPopup(textModal);
        textInput.Submitted += text =>
        {
            onSubmit(text);
            textModal.Close();
        };

        textInput.Cancelled += textModal.Close;
    }

    public void OpenPopup(Popup popup)
    {
        if (_currentPopup != null)
        {
            _popupStack.Push(_currentPopup);
        }

        _currentPopup = popup;
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

        _screen.PushTransform(-_cameraPosition);
        Surface.PaintWorldToScreen(_screen, dt);

        Surface.PaintOverlayBelowTool(_screen, HoveredWorldPosition);

        CurrentTool?.PaintToWorld(_screen);

        Surface.PaintOverlayAboveTool(_screen);
        _screen.PopTransform();

        if (_hoveredScreenPosition.HasValue)
        {
            var originalTile = _screen.GetTile(_hoveredScreenPosition.Value);

            if (UiElementAt(_hoveredScreenPosition.Value) == null)
            {
                var tile = CurrentTool?.GetTileStateInWorldOnHover(originalTile);
                if (tile.HasValue)
                {
                    _screen.PutTile(_hoveredScreenPosition.Value, tile.Value);
                }
            }
        }
        
        foreach (var uiElement in _uiElements)
        {
            var hoveredElement = uiElement.GetSubElementAt(_hoveredScreenPosition);

            if (_currentPopup != null)
            {
                hoveredElement = null;
            }
            
            uiElement.PaintSubElements(_screen, hoveredElement);
        }

        if (_currentPopup != null)
        {
            _currentPopup.PaintSubElements(_screen, _currentPopup.GetSubElementAt(_hoveredScreenPosition));
        }
        else
        {

            var keyboardChordPosition = _screen.RoomRectangle.BottomRight - new GridPosition(_chords.Count, 0);
            foreach (var chord in _chords)
            {
                _screen.PutTile(keyboardChordPosition,
                    TileState.StringCharacter(chord.FirstKey.ToString(), Color.Yellow));
                keyboardChordPosition += new GridPosition(1, 0);
            }
        }
    }

    private UiElement? UiElementAt(GridPosition hoveredTilePosition)
    {
        if (_currentPopup != null)
        {
            return _currentPopup;
        }

        for (var i = _uiElements.Count - 1; i >= 0; i--)
        {
            var uiElement = _uiElements[i];
            if (uiElement.Contains(hoveredTilePosition))
            {
                return _uiElements[i];
            }
        }

        return null;
    }

    public override void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
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
        var saveAsPopup =
            new Popup(new GridRectangle(topLeft, new GridPosition(_screen.Width - topLeft.X, topLeft.Y + 3)));
        saveAsPopup.AddStaticText(new GridPosition(1, 1), "Name this World:");
        var textInput = saveAsPopup.AddTextInput(new GridPosition(1, 2), Surface.FileName);
        OpenPopup(saveAsPopup);
        textInput.Submitted += text =>
        {
            Surface.FileName = text;
            SaveFlow();
            saveAsPopup.Close();
        };
    }

    private void FinishClosingPopup()
    {
        _currentPopup = _popupStack.Count > 0 ? _popupStack.Pop() : null;
    }

    private void ResetCameraPosition()
    {
        _cameraPosition = DefaultCameraPosition();
    }

    public T AddTool<T>(KeybindChord toolChord, Keys key, string label, T tool) where T : IEditorTool
    {
        toolChord.Add(key, label, true, _ => { _toolSelector.Selected = tool; });
        _editorTools.Add(tool);
        return tool;
    }
}
