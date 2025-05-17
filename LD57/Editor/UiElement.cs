using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;
using Microsoft.Xna.Framework;

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

    private GridPosition Transform => Rectangle.TopLeft + AdditionalTransform();

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

    protected virtual GridPosition AdditionalTransform()
    {
        return GridPosition.Zero;
    }

    public void AddDynamicText(GridPosition relativeGridPosition, Func<string> getString)
    {
        AddSubElement(new DynamicText(relativeGridPosition, getString));
    }

    public void AddInputListener(Action<ConsumableInput.ConsumableKeyboard> onInput)
    {
        AddSubElement(new InputListener(onInput));
    }

    public void AddDynamicTile(GridPosition relativeGridPosition, Func<TileState> dynamicTile)
    {
        AddSubElement(new DynamicTile(relativeGridPosition, dynamicTile));
    }

    public void AddSelectable<T>(SelectableButton<T> selectableButton) where T : class
    {
        AddSubElement(selectableButton);
    }

    public void AddButton(Button button)
    {
        AddSubElement(button);
    }

    public void AddExtraDraw(ExtraDraw extraDraw)
    {
        AddSubElement(extraDraw);
    }

    public void AddScrollListener(Func<ModifierKeys, bool> shouldScroll, Action<int> onScroll)
    {
        AddSubElement(new ScrollListener(shouldScroll, onScroll));
    }

    public DynamicImage AddDynamicImage(GridRectangle gridRectangle)
    {
        return AddSubElement(new DynamicImage(gridRectangle));
    }

    public ScrollablePane AddScrollablePane(GridRectangle viewport)
    {
        return AddSubElement(new ScrollablePane(viewport));
    }

    public T AddSubElement<T>(T subElement) where T : ISubElement
    {
        _subElements.Add(subElement);
        return subElement;
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

    public void AddStaticText(GridPosition position, string text, Color? color = null)
    {
        _subElements.Add(new DynamicText(position, () => text, color));
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

    public void UpdateMouseInput(ConsumableInput input, GridPosition hoveredScreenPosition,
        ref ISubElement? primedElement)
    {
        var hoveredElement = GetSubElementAt(hoveredScreenPosition);
        if (input.Mouse.GetButton(MouseButton.Left).WasPressed)
        {
            primedElement = hoveredElement;
        }

        if (input.Mouse.GetButton(MouseButton.Left).WasReleased)
        {
            if (primedElement == hoveredElement)
            {
                if (primedElement == null)
                {
                    OnClickedNothing();
                }

                primedElement?.OnClicked();
                primedElement = null;
            }
        }

        var scrollDelta = -input.Mouse.NormalizedScrollDelta();
        foreach (var element in _subElements)
        {
            element.OnScroll(scrollDelta, hoveredElement, input.Keyboard.Modifiers);
        }
    }

    protected virtual void OnClickedNothing()
    {
    }

    public void AddSubElements(IEnumerable<ISubElement> subElements)
    {
        foreach (var subElement in subElements)
        {
            AddSubElement(subElement);
        }
    }
}