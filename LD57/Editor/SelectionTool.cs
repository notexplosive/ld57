using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public abstract class SelectionTool : IEditorTool
{
    private readonly EditorSession _editorSession;
    private bool _isAltDown;
    private bool _isCtrlDown;
    private bool _isShiftDown;
    private GridPosition? _selectionAnchor;

    protected SelectionTool(EditorSession editorSession)
    {
        _editorSession = editorSession;
    }

    protected abstract IEditorSurface Surface { get; }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 1);

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        return original;
    }

    public string Status()
    {
        if (CanMove())
        {
            if (_isCtrlDown)
            {
                return "[CTRL] Duplicate selection";
            }

            return "Drag to move [CTRL] duplicate selection";
        }

        var altModeMessage = "";
        if (_isAltDown)
        {
            altModeMessage = "[ALT] just entities ";
        }

        if (_isCtrlDown)
        {
            return $"[CTRL] Remove {altModeMessage}from Selection";
        }

        if (_isShiftDown)
        {
            return $"[SHIFT] Append {altModeMessage}to Selection";
        }

        if (!Surface.Selection.IsEmpty)
        {
            return $"{Surface.Selection.Status()} [ESC] [F] [SHIFT] [CTRL] [ALT]";
        }

        return "[ALT]*";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition)
    {
        _isShiftDown = inputKeyboard.Modifiers.ShiftInclusive;
        _isCtrlDown = inputKeyboard.Modifiers.ControlInclusive;
        _isAltDown = inputKeyboard.Modifiers.AltInclusive;

        if (inputKeyboard.GetButton(Keys.Escape).WasPressed && !IsMoving())
        {
            Surface.Selection.Clear();
        }

        if (inputKeyboard.GetButton(Keys.F).WasPressed)
        {
            var positions = Surface.Selection.AllPositions().ToList();
            FillWithCurrentInk(positions);
            
            // rebuild selection after doing fill
            Surface.Selection.Clear();
            Surface.Selection.AddPositions(positions);
        }

        if (CanMove() && _editorSession.IsDraggingPrimary)
        {
            TranslateMoveBuffer();
            return;
        }

        if (inputKeyboard.GetButton(Keys.Delete).WasPressed)
        {
            Surface.EraseSelection();
            Surface.Selection.Clear();
        }
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            if (CanMove())
            {
                _editorSession.MoveStart = position;

                if (!_isCtrlDown)
                {
                    RemoveAllEntitiesAtSelection();
                }
            }
            else
            {
                _selectionAnchor = position;
            }
        }
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            if (CanMove())
            {
                Surface.MoveSelection();
                _editorSession.MoveStart = null;
                return;
            }

            if (position.HasValue)
            {
                if (_selectionAnchor.HasValue)
                {
                    CreateOrEditSelection(PendingSelectedPositions());
                    _selectionAnchor = null;
                }
            }
        }
    }

    public void PaintToWorld(AsciiScreen screen)
    {
        
        // paint selection
        foreach (var worldPosition in Surface.Selection.AllPositions())
        {
            screen.PutTile(worldPosition,
                Surface.Selection.GetTileStateAt(worldPosition - Surface.Selection.Offset));
        }
        
        if (_editorSession.IsDraggingPrimary)
        {
            var backgroundColor = Color.LimeGreen;
            var foregroundColor = Color.Green;

            if (_isShiftDown)
            {
                backgroundColor = Color.Yellow;
                foregroundColor = Color.Orange;
            }

            if (_isCtrlDown)
            {
                backgroundColor = Color.OrangeRed;
                foregroundColor = Color.Red;
            }

            var pendingRectangle = PendingSelectionRectangle();
            if (pendingRectangle.HasValue && _isAltDown)
            {
                // FIRST do a pass on the rectangle, in case we're in ALT mode
                foreach (var worldPosition in pendingRectangle.Value.AllPositions())
                {
                    var previousTileState = screen.GetTile(worldPosition);
                    screen.PutTile(worldPosition,
                        previousTileState with
                        {
                            BackgroundColor = Color.White,
                            BackgroundIntensity = 1f
                        });
                }
            }

            // SECOND highlight all the actual selected parts
            foreach (var worldPosition in PendingSelectedPositions())
            {
                var previousTileState = screen.GetTile(worldPosition);
                screen.PutTile(worldPosition,
                    previousTileState with
                    {
                        BackgroundColor = backgroundColor, ForegroundColor = foregroundColor, BackgroundIntensity = 1f
                    });
            }
        }
    }

    protected abstract void FillWithCurrentInk(List<GridPosition> positions);

    private GridRectangle? PendingSelectionRectangle()
    {
        if (!_selectionAnchor.HasValue || !_editorSession.HoveredWorldPosition.HasValue)
        {
            return null;
        }

        var topLeft = _selectionAnchor.Value;
        var bottomRight = _editorSession.HoveredWorldPosition.Value;
        return new GridRectangle(topLeft, bottomRight);
    }

    private IEnumerable<GridPosition> PendingSelectedPositions()
    {
        var rect = PendingSelectionRectangle();
        if (rect.HasValue)
        {
            foreach (var position in rect.Value.AllPositions())
            {
                if (_isAltDown)
                {
                    if (Surface.HasContentAt(position))
                    {
                        yield return position;
                    }
                }
                else
                {
                    yield return position;
                }
            }
        }
    }

    private void RemoveAllEntitiesAtSelection()
    {
        foreach (var position in Surface.Selection.AllPositions())
        {
            Surface.RemoveInkAt(position);
        }
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

        if (Surface.Selection.IsEmpty)
        {
            return;
        }

        var offset = _editorSession.HoveredWorldPosition.Value - _editorSession.MoveStart.Value;
        Surface.Selection.Offset = offset;
    }

    private bool CanMove()
    {
        var isSelectionHovered = _editorSession.HoveredWorldPosition.HasValue &&
                                 Surface.Selection.Contains(
                                     _editorSession.HoveredWorldPosition.Value);

        var isMakingSelection = _selectionAnchor != null;

        return !isMakingSelection && (isSelectionHovered || IsMoving());
    }

    private bool IsMoving()
    {
        return _editorSession.MoveStart.HasValue;
    }

    private void CreateOrEditSelection(IEnumerable<GridPosition> positions)
    {
        if (_isCtrlDown)
        {
            Surface.Selection.RemovePositions(positions);
            return;
        }

        if (!_isShiftDown)
        {
            Surface.Selection.Clear();
        }

        Surface.Selection.AddPositions(positions);
    }
}