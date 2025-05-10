using System;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class DynamicText : ISubElement
{
    private GridPosition Position { get; }
    private readonly Func<string> _getString;

    public DynamicText(GridPosition gridPosition, Func<string> getString)
    {
        Position = gridPosition;
        _getString = getString;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        screen.PutString(Position, _getString());
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