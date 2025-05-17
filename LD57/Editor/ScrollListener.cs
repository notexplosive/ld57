using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public class ScrollListener : ISubElement
{
    private readonly Func<ModifierKeys, bool> _shouldListen;
    private readonly Action<int> _onScroll;

    public ScrollListener(Func<ModifierKeys, bool> shouldListen, Action<int> onScroll)
    {
        _shouldListen = shouldListen;
        _onScroll = onScroll;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        
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
        if (_shouldListen(keyboardModifiers))
        {
            _onScroll(scrollDelta);
        }
    }
}
