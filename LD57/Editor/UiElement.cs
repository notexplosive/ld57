using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;

namespace LD57.Editor;

public class UiElement
{
    private readonly bool _shouldDrawFrame;
    private readonly List<ISubElement> _subElements = new();

    public UiElement(GridRectangle corners, bool shouldDrawFrame = true)
    {
        _shouldDrawFrame = shouldDrawFrame;
        Rectangle = corners;
    }

    public GridRectangle Rectangle { get; }

    public void PaintSubElements(AsciiScreen screen, ISubElement? hoveredSubElement)
    {
        if (_shouldDrawFrame)
        {
            screen.PutFrameRectangle(ResourceAlias.PopupFrame, Rectangle);
        }
        
        screen.PushTransform(Transform);
        foreach (var subElement in _subElements)
        {
            subElement.PutSubElementOnScreen(screen, hoveredSubElement);
        }
        screen.PopTransform();
    }

    private GridPosition Transform => Rectangle.TopLeft + AdditionalTransform();

    protected virtual GridPosition AdditionalTransform()
    {
        return GridPosition.Zero;
    }

    public void AddDynamicText(GridPosition relativeGridPosition, Func<string> getString)
    {
        _subElements.Add(new DynamicText(relativeGridPosition, getString));
    }

    public void AddInputListener(Action<ConsumableInput.ConsumableKeyboard> onInput)
    {
        _subElements.Add(new InputListener(onInput));
    }

    public void AddDynamicTile(GridPosition relativeGridPosition, Func<TileState> dynamicTile)
    {
        _subElements.Add(new DynamicTile(relativeGridPosition, dynamicTile));
    }

    public void AddSelectable<T>(SelectableButton<T> selectableButton) where T : class
    {
        _subElements.Add(selectableButton);
    }

    public void AddButton(Button button)
    {
        _subElements.Add(button);
    }

    public DynamicImage AddDynamicImage(GridRectangle gridRectangle)
    {
        var dynamicImage = new DynamicImage(gridRectangle); 
        _subElements.Add(dynamicImage);
        return dynamicImage;
    }

    public ScrollablePane AddScrollablePane(GridRectangle viewport)
    {
        var pane = new ScrollablePane(viewport);
        _subElements.Add(pane);
        return pane;
    }

    public ISubElement? GetSubElementAt(GridPosition? absoluteScreenPosition)
    {
        if (!absoluteScreenPosition.HasValue)
        {
            return null;
        }
        
        foreach (var subElement in _subElements)
        {
            var relativePosition = absoluteScreenPosition.Value - Transform;
            if (subElement.Contains(relativePosition))
            {
                if (subElement is UiElement subUiElement)
                {
                    return subUiElement.GetSubElementAt(relativePosition);
                }
                
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
        _subElements.Add(new DynamicText(position, () => text));
    }

    public TextInputElement AddTextInput(GridPosition gridPosition, string? text)
    {
        var textInputElement = new TextInputElement(gridPosition, text);
        _subElements.Add(textInputElement);
        return textInputElement;
    }

    public void OnTextInput(char[] enteredCharacters)
    {
        foreach (var element in _subElements)
        {
            element.OnTextInput(enteredCharacters);
        }
    }

    public void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        foreach (var subElement in _subElements)
        {
            subElement.UpdateKeyboardInput(inputKeyboard);
        }
    }

    
    public void UpdateMouseInput(ConsumableInput.ConsumableMouse inputMouse, GridPosition hoveredScreenPosition, ref ISubElement? primedElement)
    {
        var hoveredElement = GetSubElementAt(hoveredScreenPosition);
        if (inputMouse.GetButton(MouseButton.Left).WasPressed)
        {
            primedElement = hoveredElement;
        }
        
        if (inputMouse.GetButton(MouseButton.Left).WasReleased)
        {
            if (primedElement == hoveredElement)
            {
                primedElement?.OnClicked();
                primedElement = null;
            }
        }
        
        var scrollDelta = -inputMouse.NormalizedScrollDelta();
        foreach (var element in _subElements)
        {
            element.OnScroll(scrollDelta, hoveredElement);
        }
    }
}