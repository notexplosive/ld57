using System;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class InputListener : ISubElement
{
    private readonly Action<ConsumableInput.ConsumableKeyboard> _onInput;

    public InputListener(Action<ConsumableInput.ConsumableKeyboard> onInput)
    {
        _onInput = onInput;
    }

    public void PutOnScreen(AsciiScreen screen, GridPosition topLeft)
    {
        
    }

    public bool Contains(GridPosition relativePosition)
    {
        return false;
    }

    public void ShowHover(AsciiScreen screen, GridPosition hoveredTilePosition, GridPosition topLeft)
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
        _onInput(inputKeyboard);
    }
}
