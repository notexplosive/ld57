using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class SelectionTool : IEditorTool
{
    private readonly EditorSession _editorSession;

    public SelectionTool(EditorSession editorSession)
    {
        _editorSession = editorSession;
    }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 1);

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        return original;
    }

    public string Status()
    {
        if (!CanMove())
        {
            if (_editorSession.SelectionRectangle.HasValue)
            {
                return "[F]ill";
            }
        }
        else

        {
            return "Drag to move";
        }

        return "Selection";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        if (!CanMove())
        {
            if (_editorSession.SelectionRectangle != null)
            {
                if (_editorSession.SelectedTemplate != null)
                {
                    if (inputKeyboard.GetButton(Keys.F).WasPressed)
                    {
                        _editorSession.WorldTemplate.FillRectangle(_editorSession.SelectionRectangle.Value,
                            _editorSession.SelectedTemplate);
                    }
                }

                if (inputKeyboard.GetButton(Keys.Delete).WasPressed)
                {
                    _editorSession.WorldTemplate.EraseRectangle(_editorSession.SelectionRectangle.Value);
                }
            }
        }
        else

        {
            if (_editorSession.MoveStart != null && _editorSession.HoveredWorldPosition.HasValue &&
                _editorSession.SelectionRectangle != null)
            {
                var offset = _editorSession.HoveredWorldPosition.Value - _editorSession.MoveStart.Value;
                _editorSession.SelectionRectangle = _editorSession.SelectionRectangle.Value.Moved(offset);

                foreach (var item in _editorSession.MoveBuffer)
                {
                    item.Position += offset;
                }

                _editorSession.MoveStart = _editorSession.HoveredWorldPosition.Value;
            }
        }
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            if (!CanMove())
            {
                _editorSession.SelectionAnchor = position;
            }
            else
            {
                _editorSession.MoveStart = position;
            }
        }
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (!CanMove())
        {
            if (position.HasValue)
            {
                _editorSession.CreateSelection(position.Value);
            }
        }
        else
        {
            foreach (var item in _editorSession.MoveBuffer)
            {
                _editorSession.WorldTemplate.RemoveEntitiesAt(item.Position);
            }

            foreach (var item in _editorSession.MoveBuffer)
            {
                _editorSession.WorldTemplate.AddExactEntity(item);
            }

            _editorSession.MoveStart = null;
        }
    }

    private bool CanMove()
    {
        // todo!
        return false;
    }
}
