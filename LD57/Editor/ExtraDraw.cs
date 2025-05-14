using System;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class ExtraDraw : ISubElement
{
    private readonly Action<AsciiScreen> _extraDrawFunction;

    public ExtraDraw(Action<AsciiScreen> extraDrawFunction)
    {
        _extraDrawFunction = extraDrawFunction;
    }
    
    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        _extraDrawFunction(screen);
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
