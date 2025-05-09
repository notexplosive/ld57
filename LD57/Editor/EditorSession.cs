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
    private readonly EditorSelector<IEditorTool> _toolSelector = new();
    private readonly List<UiElement> _uiElements = new();
    private GridPosition _cameraPosition;
    private UiElement? _currentPopup;
    private GridPosition? _hoveredScreenPosition;
    private readonly Stack<UiElement> _popupStack = new();
    private ISubElement? _primedElement;
    private AsciiScreen _screen;
    private bool _shouldClosePopup;

    public EditorSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem, IEditorSurface surface) : base(
        runtimeWindow,
        runtimeFileSystem)
    {
        _screen = RebuildScreenWithWidth(46);
        Surface = surface;
        Surface.RequestResetCamera += ResetCameraPosition;
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

    public List<IEditorTool> EditorTools { get; } = new();
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
                if (HitUiElement(_hoveredScreenPosition.Value) != null)
                {
                    return null;
                }
            }

            return _cameraPosition + _hoveredScreenPosition;
        }
    }

    private IEditorTool? CurrentTool => _toolSelector.Selected;
    public List<Action<ConsumableInput, int>> ExtraKeyBinds { get; } = new();

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
            new UiElement(new GridRectangle(new GridPosition(0, 0), new GridPosition(3, EditorTools.Count + 2)));

        var toolIndex = 0;
        foreach (var tool in EditorTools)
        {
            leftToolbar.AddSelectable(new SelectableButton<IEditorTool>(new GridPosition(1, 1 + toolIndex),
                tool.TileStateInToolbar, _toolSelector, tool));
            toolIndex++;
        }

        _uiElements.Add(leftToolbar);

        var statusBar =
            new UiElement(new GridRectangle(bottomLeftCorner, new GridPosition(screen.Width, screen.Height)));
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

        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            _primedElement = HitSubElement();

            if (HoveredWorldPosition.HasValue)
            {
                StartMousePressInWorld(HoveredWorldPosition.Value, MouseButton.Left);
            }
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            if (_primedElement == HitSubElement())
            {
                _primedElement?.OnClicked();
                _primedElement = null;
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
            FinishClosingPopup();
            _shouldClosePopup = false;
        }

        _hoveredScreenPosition = _screen.GetHoveredTile(input, hitTestStack, Vector2.Zero);

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

        CurrentTool?.UpdateInput(input.Keyboard, HoveredWorldPosition);

        if (!IsDraggingPrimary && !IsDraggingSecondary)
        {
            var scrollVector = new Vector2(0, input.Mouse.ScrollDelta());
            if (scrollVector.Y != 0)
            {
                var scrollDelta = (int) scrollVector.Normalized().Y;
                var flippedDelta = -scrollDelta;

                if (input.Keyboard.Modifiers.Control)
                {
                    if (CurrentTool != null)
                    {
                        var currentIndex = EditorTools.IndexOf(CurrentTool);
                        var newIndex = Math.Clamp(currentIndex + flippedDelta, 0, EditorTools.Count - 1);
                        _toolSelector.Selected = EditorTools[newIndex];
                    }
                }

                foreach (var extraKeyBindEvent in ExtraKeyBinds)
                {
                    extraKeyBindEvent(input, flippedDelta);
                }
            }
        }
    }

    private ISubElement? HitSubElement()
    {
        if (!_hoveredScreenPosition.HasValue)
        {
            return null;
        }

        var hitUiElement = HitUiElement(_hoveredScreenPosition.Value);
        if (hitUiElement == null ||
            (!hitUiElement.Contains(_hoveredScreenPosition.Value) && hitUiElement != _currentPopup))
        {
            return null;
        }

        var probedPosition = _hoveredScreenPosition.Value - hitUiElement.Rectangle.TopLeft;
        return hitUiElement.GetSubElementAt(probedPosition);

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
            new Popup(new GridRectangle(topLeft, new GridPosition(_screen.Width - topLeft.X, topLeft.Y + 4)));
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
        popup.RequestClosePopup += StartClosingPopup;

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

        Surface.PaintWorldToScreen(_screen, _cameraPosition, dt);

        // paint selection
        foreach (var worldPosition in Surface.Selection.AllPositions())
        {
            var screenPosition = worldPosition - _cameraPosition;
            if (_screen.ContainsPosition(screenPosition))
            {
                _screen.PutTile(screenPosition, Surface.Selection.GetTileStateAt(worldPosition - Surface.Selection.Offset));
            }
        }
        
        Surface.PaintOverlayBelowTool(_screen, _cameraPosition, HoveredWorldPosition);

        CurrentTool?.PaintToWorld(_screen, _cameraPosition);

        Surface.PaintOverlayAboveTool(_screen, _cameraPosition);

        foreach (var uiElement in _uiElements)
        {
            uiElement.PaintToScreen(_screen);
        }

        if (_currentPopup != null)
        {
            _currentPopup.PaintToScreen(_screen);
        }
        
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
                var hoveredSubElement = uiElement.GetSubElementAt(_hoveredScreenPosition.Value - uiElement.Rectangle.TopLeft);
                if (hoveredSubElement != null)
                {
                    hoveredSubElement.ShowHover(_screen, _hoveredScreenPosition.Value, uiElement.Rectangle.TopLeft);
                }
            }
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
            new Popup(new GridRectangle(topLeft, new GridPosition(_screen.Width - topLeft.X, topLeft.Y + 4)));
        saveAsPopup.AddStaticText(new GridPosition(1, 1), "Name this World:");
        var textInput = saveAsPopup.AddTextInput(new GridPosition(1, 2), Surface.FileName);
        OpenPopup(saveAsPopup);
        textInput.Submitted += text =>
        {
            Surface.FileName = text;
            SaveFlow();
            StartClosingPopup();
        };
    }

    private void StartClosingPopup()
    {
        _shouldClosePopup = true;
    }

    private void FinishClosingPopup()
    {
        _currentPopup = _popupStack.Count > 0 ? _popupStack.Pop() : null;
    }

    private void ResetCameraPosition()
    {
        _cameraPosition = DefaultCameraPosition();
    }
}
