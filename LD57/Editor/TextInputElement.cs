using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class TextInputElement : ISubElement
{
    private readonly GridPosition _gridPosition;
    private string _textBuffer = string.Empty;
    private Color? _backgroundColor;

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
        screen.PutString(_gridPosition, _textBuffer, ResourceAlias.Color("default"), _backgroundColor);

        // draw cursor
        var cursorTile = TileState.Sprite(ResourceAlias.Tools, 19);

        if (_backgroundColor.HasValue)
        {
            cursorTile = cursorTile.WithBackground(_backgroundColor.Value);
        }

        screen.PutTile(_gridPosition + new GridPosition(_textBuffer.Length, 0), cursorTile);
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
                    if (_textBuffer.Length > 0)
                    {
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

    public void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        if (inputKeyboard.GetButton(Keys.Escape).WasPressed)
        {
            Cancelled?.Invoke();
        }
    }

    public void OnScroll(int scrollDelta, ISubElement? hoveredElement, ModifierKeys keyboardModifiers)
    {
    }

    public event Action<string>? Submitted;
    public event Action? Cancelled;

    public void SetBackgroundColor(Color backgroundColor)
    {
        _backgroundColor = backgroundColor;
    }

    public void Clear()
    {
        _textBuffer = string.Empty;
    }
}
