using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public class DynamicImage : ISubElement
{
    private readonly GridRectangle _gridRectangle;
    private Action<AsciiScreen>? _doDraw;

    public DynamicImage(GridRectangle gridRectangle)
    {
        _gridRectangle = gridRectangle;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        screen.PushStencil(_gridRectangle);
        screen.PushTransform(_gridRectangle.TopLeft);
        _doDraw?.Invoke(screen);
        screen.PopTransform();
        screen.PopStencil();
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

    public void OnScroll(int scrollDelta, ISubElement? hoveredElement, ModifierKeys keyboardModifiers)
    {
        
    }

    public DynamicImage SetDrawAction(Action<AsciiScreen> doDraw)
    {
        _doDraw = doDraw;
        return this;
    }
}
