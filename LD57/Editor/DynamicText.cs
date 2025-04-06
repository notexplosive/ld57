using System;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class DynamicText : ISubElement
{
    public GridPosition Position { get; }
    private readonly Func<string> _getString;

    public string GetString()
    {
        return _getString();
    }

    public DynamicText(GridPosition gridPosition, Func<string> getString)
    {
        Position = gridPosition;
        _getString = getString;
    }

    public void PutOnScreen(AsciiScreen screen, GridPosition topLeft)
    {
        screen.PutString(Position + topLeft, GetString());
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
