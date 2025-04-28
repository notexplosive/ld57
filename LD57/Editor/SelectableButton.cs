using System;
using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class SelectableButton : ISubElement
{
    private readonly GridPosition _gridPosition;
    private readonly Action _onSelect;
    private readonly EditorSelector _selector;
    private readonly TileState _tileState;

    public SelectableButton(GridPosition gridPosition, TileState tileState, EditorSelector selector, Action onSelect)
    {
        _gridPosition = gridPosition;
        _tileState = tileState;
        _selector = selector;
        _onSelect = onSelect;

        if (_selector.Selected == null)
        {
            _selector.Selected = this;
        }
    }

    public void PutOnScreen(AsciiScreen screen, GridPosition topLeft)
    {
        var renderedTileState = _tileState;
        if (_selector.Selected == this)
        {
            renderedTileState = renderedTileState with
            {
                BackgroundColor = Color.White,
                BackgroundIntensity = 1f,
                ForegroundColor = Color.Blue
            };
        }

        screen.PutTile(_gridPosition + topLeft, renderedTileState);
    }

    public bool Contains(GridPosition position)
    {
        return position == _gridPosition;
    }

    public void ShowHover(AsciiScreen screen, GridPosition hoveredTilePosition)
    {
        var newTile = screen.GetTile(hoveredTilePosition) with {BackgroundColor = Color.LightBlue, BackgroundIntensity = 1f};
        screen.PutTile(hoveredTilePosition, newTile);
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
        _selector.Selected = this;
    }

    public void OnSelect()
    {
        _onSelect();
    }
}
