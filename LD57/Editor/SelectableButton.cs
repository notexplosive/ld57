using System;
using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class SelectableButton<T> : ISubElement where T : class
{
    private readonly GridPosition _gridPosition;
    private readonly EditorSelector<T> _selector;
    private readonly T _selectableContent;
    private readonly TileState _tileState;

    public SelectableButton(GridPosition gridPosition, TileState tileState, EditorSelector<T> selector, T selectableContent)
    {
        _gridPosition = gridPosition;
        _tileState = tileState;
        _selector = selector;
        _selectableContent = selectableContent;
        _selector.Selected ??= selectableContent;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        var renderedTileState = _tileState;
        if (_selector.IsSelected(_selectableContent))
        {
            renderedTileState = renderedTileState with
            {
                BackgroundColor = Color.White,
                ForegroundColor = Color.Black,
                BackgroundIntensity = 1f,
            };
        }

        screen.PutTile(_gridPosition, renderedTileState);

        if (hoveredElement == this)
        {
            var newTile = screen.GetTile(_gridPosition) with
            {
                BackgroundColor = Color.LightBlue, BackgroundIntensity = 1f
            };
            screen.PutTile(_gridPosition, newTile);
        }
    }

    public bool Contains(GridPosition relativePosition)
    {
        return relativePosition == _gridPosition;
    }

    public void ShowHover(AsciiScreen screen, GridPosition hoveredTilePosition)
    {

    }

    public void OnClicked()
    {
        Select();
    }

    public void OnTextInput(char[] enteredCharacters)
    {
        
    }

    public void UpdateKeyboardInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        
    }

    public void Select()
    {
        _selector.Selected = _selectableContent;
    }
}
