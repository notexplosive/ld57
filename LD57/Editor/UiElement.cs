using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class UiElement
{
    private List<ISubElement> _subElement = new();

    public UiElement(GridPosition topLeft, GridPosition bottomRight)
    {
        TopLeft = topLeft;
        BottomRight = bottomRight;
    }

    public GridPosition BottomRight { get; }
    public GridPosition TopLeft { get; }
    public int Width => Rectangle.Width;
    public int Height => Rectangle.Height;

    public Rectangle Rectangle => Constants.CreateRectangle(TopLeft, BottomRight);

    public void PaintToScreen(AsciiScreen screen)
    {
        screen.PutFrameRectangle(ResourceAlias.PopupFrame, TopLeft, BottomRight);

        foreach (var subElement in _subElement)
        {
            subElement.PutOnScreen(screen, TopLeft);
        }
    }

    public void AddDynamicText(GridPosition relativeGridPosition, Func<string> getString)
    {
        _subElement.Add(new DynamicText(relativeGridPosition, getString));
    }

    public void AddSelectable<T>(SelectableButton<T> selectableButton) where T : class
    {
        _subElement.Add(selectableButton);
    }

    public ISubElement? GetSubElementAt(GridPosition position)
    {
        foreach (var subElement in _subElement)
        {
            if (subElement.Contains(position - TopLeft))
            {
                return subElement;
            }
        }

        return null;
    }

    public bool Contains(GridPosition position)
    {
        return Rectangle.Contains(position.ToPoint());
    }

    public void AddStaticText(GridPosition position, string text)
    {
        _subElement.Add(new DynamicText(position, ()=>text));
    }

    public TextInputElement AddTextInput(GridPosition gridPosition, string? text)
    {
        var textInputElement = new TextInputElement(gridPosition, text);
        _subElement.Add(textInputElement);
        return textInputElement;
    }

    public void OnTextInput(char[] enteredCharacters)
    {
        foreach (var element in _subElement)
        {
            element.OnTextInput(enteredCharacters);
        }
    }

    public void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        foreach (var subElement in _subElement)
        {
            subElement.UpdateKeyboardInput(inputKeyboard);
        }
    }
}