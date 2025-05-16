using System;
using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class DynamicText : ISubElement
{
    private GridPosition Position { get; }
    private readonly Func<string> _getString;
    private readonly Color? _color;

    public DynamicText(GridPosition gridPosition, Func<string> getString, Color? color = null)
    {
        Position = gridPosition;
        _getString = getString;
        _color = color;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        screen.PutString(Position, _getString(), _color);
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

    public void OnScroll(int scrollDelta, ISubElement? hoveredElement)
    {
        
    }
}