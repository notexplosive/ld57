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
        if (CanMove())
        {
            return $"Drag to move";
        }

        if (!_editorSession.WorldSelection.IsEmpty)
        {
            return $"[F]ill; {_editorSession.WorldSelection.Status()}";
        }

        return "Selection";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        if (inputKeyboard.GetButton(Keys.Escape).WasPressed && !IsMoving())
        {
            _editorSession.WorldSelection.Clear();
        }

        if (CanMove())
        {
            TranslateMoveBuffer();
            return;
        }

        if (_editorSession.SelectedTemplate != null)
        {
            if (inputKeyboard.GetButton(Keys.F).WasPressed)
            {
                _editorSession.WorldTemplate.FillAllPositions(_editorSession.WorldSelection.AllPositions(),
                    _editorSession.SelectedTemplate);
            }
        }

        if (inputKeyboard.GetButton(Keys.Delete).WasPressed)
        {
            _editorSession.WorldTemplate.EraseAtPositions(_editorSession.WorldSelection.AllPositions());
        }
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            if (CanMove())
            {
                _editorSession.MoveStart = position;
                RemoveAllEntitiesAtSelection();
            }
            else
            {
                _editorSession.SelectionAnchor = position;
            }
        }
    }

    private void RemoveAllEntitiesAtSelection()
    {
        foreach (var position in _editorSession.WorldSelection.AllPositions())
        {
            _editorSession.WorldTemplate.RemoveEntitiesAt(position);
        }
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (!CanMove())
        {
            if (position.HasValue)
            {
                CreateSelection(position.Value);
            }

            return;
        }
        
        foreach (var item in _editorSession.WorldSelection.AllPositions())
        {
            _editorSession.WorldTemplate.RemoveEntitiesAt(item);
        }

        foreach (var item in _editorSession.WorldSelection.AllEntitiesWithCurrentPlacement())
        {
            _editorSession.WorldTemplate.AddExactEntity(item);
        }

        _editorSession.WorldSelection.RegenerateAtNewPosition(_editorSession);
        _editorSession.MoveStart = null;
    }

    private void TranslateMoveBuffer()
    {
        if (_editorSession.MoveStart == null)
        {
            return;
        }

        if (!_editorSession.HoveredWorldPosition.HasValue)
        {
            return;
        }

        if (_editorSession.WorldSelection.IsEmpty)
        {
            return;
        }

        var offset = _editorSession.HoveredWorldPosition.Value - _editorSession.MoveStart.Value;
        _editorSession.WorldSelection.Offset = offset;
    }

    private bool CanMove()
    {
        var isSelectionHovered = _editorSession.HoveredWorldPosition.HasValue &&
                                 _editorSession.WorldSelection.Contains(
                                     _editorSession.HoveredWorldPosition.Value);

        return isSelectionHovered || IsMoving();
    }

    private bool IsMoving()
    {
        return _editorSession.MoveStart.HasValue;
    }

    public void CreateSelection(GridPosition releasedPosition)
    {
        if (!_editorSession.SelectionAnchor.HasValue)
        {
            return;
        }

        _editorSession.WorldSelection.Clear();
        _editorSession.WorldSelection.AddRectangle(_editorSession,
            new GridPositionCorners(releasedPosition, _editorSession.SelectionAnchor.Value));
        _editorSession.SelectionAnchor = null;
    }
}
