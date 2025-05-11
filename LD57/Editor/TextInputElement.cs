using System;
using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class TextInputElement : ISubElement
{
    private readonly GridPosition _gridPosition;
    private string _textBuffer = string.Empty;

    public TextInputElement(GridPosition gridPosition, string? startingText)
    {
        if (!string.IsNullOrEmpty(startingText))
        {
            _textBuffer = startingText;
        }
        _gridPosition = gridPosition;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        screen.PutString(_gridPosition, _textBuffer);
        screen.PutTile(_gridPosition + new GridPosition(_textBuffer.Length, 0), TileState.Sprite(ResourceAlias.Walls, 0));
    }

    public bool Contains(GridPosition relativePosition)
    {
        return new GridRectangle(_gridPosition, _gridPosition + new GridPosition(0, _textBuffer.Length))
            .Contains(relativePosition, true);
    }

    public void OnClicked()
    {
        
    }

    public void OnTextInput(char[] enteredCharacters)
    {
        foreach (var character in enteredCharacters)
        {
            if (!char.IsControl(character))
            {
                _textBuffer += character;
            }
            else
            {
                if (character == '\b')
                {
                    if(_textBuffer.Length > 0){
                        _textBuffer = _textBuffer.Substring(0, _textBuffer.Length - 1);
                    }
                }

                if (character == '\r')
                {
                    Submitted?.Invoke(_textBuffer);
                }
            }
        }
    }

    public event Action<string>? Submitted;
    public event Action? Cancelled;

    public void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        if (inputKeyboard.GetButton(Keys.Escape).WasPressed)
        {
            Cancelled?.Invoke();
        }
    }

    public void OnScroll(int scrollDelta, ISubElement? hoveredElement)
    {
        
    }
}
