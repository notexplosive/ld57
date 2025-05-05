using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class SelectionTool : IEditorTool
{
    private readonly EditorSession _editorSession;
    private readonly WorldEditorSurface _surface;
    private readonly Func<EntityTemplate?> _getTemplate;
    private bool _isAltDown;
    private bool _isCtrlDown;
    private bool _isShiftDown;
    private GridPosition? _selectionAnchor;

    public SelectionTool(EditorSession editorSession, WorldEditorSurface surface, Func<EntityTemplate?> getTemplate)
    {
        _editorSession = editorSession;
        _surface = surface;
        _getTemplate = getTemplate;
    }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 1);

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        return original;
    }

    public string Status()
    {
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

        
        if (!_surface.Selection.IsEmpty)
        {
            return $"{_surface.Selection.Status()} [ESC] [F] [SHIFT] [CTRL] [ALT]";
        }

        return "[ALT]*";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
        _isShiftDown = inputKeyboard.Modifiers.ShiftInclusive;
        _isCtrlDown = inputKeyboard.Modifiers.ControlInclusive;
        _isAltDown = inputKeyboard.Modifiers.AltInclusive;

        if (inputKeyboard.GetButton(Keys.Escape).WasPressed && !IsMoving())
        {
            _surface.Selection.Clear();
        }

        var entityTemplate = _getTemplate();
        if (entityTemplate != null)
        {
            if (inputKeyboard.GetButton(Keys.F).WasPressed)
            {
                var positions = _surface.Selection.AllPositions().ToList();
                _surface.WorldTemplate.FillAllPositions(positions, entityTemplate);
                _surface.Selection.Clear();
                CreateOrEditSelection(positions);
            }
        }
        
        if (CanMove() && _editorSession.IsDraggingPrimary)
        {
            TranslateMoveBuffer();
            return;
        }

        if (inputKeyboard.GetButton(Keys.Delete).WasPressed)
        {
            _surface.WorldTemplate.EraseAtPositions(_surface.Selection.AllPositions());
            _surface.Selection.Clear();
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
                foreach (var item in _surface.Selection.AllPositions())
                {
                    _surface.WorldTemplate.RemoveEntitiesAt(item);
                }

                foreach (var item in _surface.Selection.AllEntitiesWithCurrentPlacement())
                {
                    _surface.WorldTemplate.AddExactEntity(item);
                }

                _surface.Selection.RegenerateAtNewPosition();
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

    public void PaintToScreen(AsciiScreen screen, GridPosition cameraPosition)
    {
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
                foreach (var worldPosition in pendingRectangle.Value.AllPositions(true))
                {
                    var screenPosition = worldPosition - cameraPosition;
                    var previousTileState = screen.GetTile(screenPosition);
                    screen.PutTile(screenPosition,
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
                var screenPosition = worldPosition - cameraPosition;
                var previousTileState = screen.GetTile(screenPosition);
                screen.PutTile(screenPosition,
                    previousTileState with
                    {
                        BackgroundColor = backgroundColor, ForegroundColor = foregroundColor, BackgroundIntensity = 1f
                    });
            }
        }
    }

    private GridPositionCorners? PendingSelectionRectangle()
    {
        if (!_selectionAnchor.HasValue || !_editorSession.HoveredWorldPosition.HasValue)
        {
            return null;
        }
        var topLeft = _selectionAnchor.Value;
        var bottomRight = _editorSession.HoveredWorldPosition.Value;
        return new GridPositionCorners(topLeft, bottomRight);
    }
    
    private IEnumerable<GridPosition> PendingSelectedPositions()
    {
        var rect = PendingSelectionRectangle();
        if (rect.HasValue)
        {
            foreach (var position in rect.Value.AllPositions(true))
            {
                if (_isAltDown)
                {
                    if (_surface.WorldTemplate.HasEntityAt(position))
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
        foreach (var position in _surface.Selection.AllPositions())
        {
            _surface.WorldTemplate.RemoveEntitiesAt(position);
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

        if (_surface.Selection.IsEmpty)
        {
            return;
        }

        var offset = _editorSession.HoveredWorldPosition.Value - _editorSession.MoveStart.Value;
        _surface.Selection.Offset = offset;
    }

    private bool CanMove()
    {
        var isSelectionHovered = _editorSession.HoveredWorldPosition.HasValue &&
                                 _surface.Selection.Contains(
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
            _surface.Selection.RemovePositions(positions);
            return;
        }

        if (!_isShiftDown)
        {
            _surface.Selection.Clear();
        }

        _surface.Selection.AddPositions(positions);
    }
}
