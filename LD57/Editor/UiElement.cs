using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class UiElement
{
    public GridRectangle Rectangle { get; }
    private List<ISubElement> _subElement = new();

    public UiElement(GridRectangle corners)
    {
        Rectangle = corners;
    }

    public void PaintToScreen(AsciiScreen screen)
    {
        screen.PutFrameRectangle(ResourceAlias.PopupFrame, Rectangle);

        foreach (var subElement in _subElement)
        {
            subElement.PutOnScreen(screen, Rectangle.TopLeft);
        }
    }

    public void AddDynamicText(GridPosition relativeGridPosition, Func<string> getString)
    {
        _subElement.Add(new DynamicText(relativeGridPosition, getString));
    }
    
    public void AddInputListener(Action<ConsumableInput.ConsumableKeyboard> onInput)
    {
        _subElement.Add(new InputListener(onInput));
    }

    public void AddDynamicTile(GridPosition relativeGridPosition, Func<TileState> dynamicTile)
    {
        _subElement.Add(new DynamicTile(relativeGridPosition, dynamicTile));
    }

    public void AddSelectable<T>(SelectableButton<T> selectableButton) where T : class
    {
        _subElement.Add(selectableButton);
    }
    
    public void AddButton(Button button)
    {
        _subElement.Add(button);
    }

    public ISubElement? GetSubElementAt(GridPosition position)
    {
        foreach (var subElement in _subElement)
        {
            if (subElement.Contains(position))
            {
                return subElement;
            }
        }

        return null;
    }

    public bool Contains(GridPosition position)
    {
        return Rectangle.Contains(position, true);
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