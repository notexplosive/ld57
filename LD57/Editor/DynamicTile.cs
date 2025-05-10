using System;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class DynamicTile : ISubElement
{
    private GridPosition Position { get; }
    private readonly Func<TileState> _getTile;

    public DynamicTile(GridPosition gridPosition, Func<TileState> getTile)
    {
        Position = gridPosition;
        _getTile = getTile;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, bool isHovered)
    {
        screen.PutTile(Position, _getTile());
    }

    public bool Contains(GridPosition relativePosition)
    {
        return false;
    }

    public void OnClicked()
    {
        
    }

    public void OnTextInput(char[] enteredCharacters)
    {
        
    }

    public void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        
    }
}
