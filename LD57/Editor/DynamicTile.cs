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

    public void PutOnScreen(AsciiScreen screen, GridPosition topLeft)
    {
        screen.PutTile(Position + topLeft, _getTile());
    }

    public bool Contains(GridPosition position)
    {
        return false;
    }

    public void ShowHover(AsciiScreen screen, GridPosition hoveredTilePosition)
    {
        
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
