using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public class Button : ISubElement
{
    private readonly Action _onClick;
    private readonly GridPosition _position;
    private Func<TileState>? _getTileState;
    private Func<TileState?>? _getTileStateOnHover;

    public Button(GridPosition position, Action onClick)
    {
        _position = position;
        _onClick = onClick;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        var getter = GetTileState();
        if (getter.HasValue)
        {
            screen.PutTile(_position, getter.Value);
        }
        
        if (hoveredElement == this)
        {
            var hoverGetter = GetTileStateOnHover();
            if (hoverGetter.HasValue)
            {
                screen.PutTile(_position, hoverGetter.Value);
            }
        }
    }

    public bool Contains(GridPosition relativePosition)
    {
        return relativePosition == _position;
    }

    public void ShowHover(AsciiScreen screen, GridPosition hoveredTilePosition)
    {
        
    }

    public void OnClicked()
    {
        _onClick();
    }

    public void OnTextInput(char[] enteredCharacters)
    {
    }

    public void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
    }

    public void OnScroll(int scrollDelta, ISubElement? hoveredElement, ModifierKeys keyboardModifiers)
    {
        
    }

    public Button SetTileStateGetter(Func<TileState> getTileState)
    {
        _getTileState = getTileState;
        return this;
    }

    public Button SetTileStateOnHoverGetter(Func<TileState?> getTileState)
    {
        _getTileStateOnHover = getTileState;
        return this;
    }

    private TileState? GetTileState()
    {
        return _getTileState?.Invoke();
    }

    private TileState? GetTileStateOnHover()
    {
        return _getTileStateOnHover?.Invoke();
    }
}
