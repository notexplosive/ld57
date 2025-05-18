using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.Editor;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.CartridgeManagement;

public class CanvasMetadataTool : IEditorTool
{
    private readonly CanvasEditorSurface _canvasSurface;
    private readonly EditorSession _editorSession;
    private string? _draggedExtraData;
    private GridPosition? _dragStart;

    public CanvasMetadataTool(EditorSession editorSession, CanvasEditorSurface canvasSurface)
    {
        _editorSession = editorSession;
        _canvasSurface = canvasSurface;
    }

    public TileState TileStateInToolbar { get; } = TileState.Sprite(ResourceAlias.Tools, 5);

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        var newSprite = TileState.Sprite(ResourceAlias.Floors, 7);

        if (_editorSession.HoveredWorldPosition.HasValue)
        {
            if (GetExtraDataAt(_editorSession.HoveredWorldPosition.Value) != null)
            {
                return TileState.Sprite(ResourceAlias.Floors, 8).WithForeground(Color.OrangeRed);
            }
        }

        if (_draggedExtraData != null)
        {
            return newSprite.WithSprite(ResourceAlias.Tools, 2).WithForeground(Color.OrangeRed);
        }

        return newSprite;
    }

    public string Status()
    {
        if (_editorSession.HoveredWorldPosition.HasValue)
        {
            var ink = _canvasSurface.Data.InkAt(_editorSession.HoveredWorldPosition.Value);
            if (ink != null && ink.CanvasTileData.HasExtraData())
            {
                return ink.CanvasTileData.ExtraData;
            }
        }

        return "Metadata";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard, GridPosition? hoveredWorldPosition)
    {
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            var foundInk = _canvasSurface.Data.InkAt(position);

            if (foundInk != null && foundInk.CanvasTileData.HasExtraData())
            {
                _draggedExtraData = foundInk.CanvasTileData.ExtraData;
                _dragStart = position;
                RemoveExtraDataAt(position);
            }
        }
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            if (_draggedExtraData != null && _dragStart.HasValue)
            {
                if (position.HasValue)
                {
                    PutExtraDataAt(_draggedExtraData, position.Value);

                    if (_dragStart == position.Value)
                    {
                        EditAt(position.Value);
                    }
                }
                else
                {
                    PutExtraDataAt(_draggedExtraData, _dragStart.Value);
                }

                _draggedExtraData = null;
                _dragStart = null;
            }
            else if (position.HasValue)
            {
                EditAt(position.Value);
            }
        }
    }

    public void PaintToWorld(AsciiScreen screen)
    {
        foreach (var tile in _canvasSurface.Data.Content.Where(a => a.CanvasTileData.HasExtraData()))
        {
            screen.PutTile(tile.Position, TileState.StringCharacter("!", Color.OrangeRed));
        }
    }

    private string? GetExtraDataAt(GridPosition position)
    {
        var ink = _canvasSurface.Data.InkAt(position);
        if (ink != null && ink.CanvasTileData.HasExtraData())
        {
            return ink.CanvasTileData.ExtraData;
        }

        return null;
    }

    private void RemoveExtraDataAt(GridPosition position)
    {
        var foundInk = _canvasSurface.Data.InkAt(position);
        if (foundInk != null)
        {
            foundInk.CanvasTileData = foundInk.CanvasTileData with {ExtraData = string.Empty};
        }
    }

    private void EditAt(GridPosition position)
    {
        var startingText = string.Empty;

        var foundInk = _canvasSurface.Data.InkAt(position);
        if (foundInk != null)
        {
            startingText = foundInk.CanvasTileData.ExtraData;
        }

        _editorSession.RequestText("Enter Command", startingText,
            submittedText => { PutExtraDataAt(submittedText, position); });
    }

    private void PutExtraDataAt(string submittedText, GridPosition position)
    {
        if (string.IsNullOrEmpty(submittedText))
        {
            submittedText = string.Empty;
        }

        var foundInk = _canvasSurface.Data.InkAt(position);

        if (foundInk != null)
        {
            foundInk.CanvasTileData = foundInk.CanvasTileData with {ExtraData = submittedText};
        }
        else
        {
            _canvasSurface.Data.AddExact(new PlacedCanvasTile
            {
                Position = position,
                CanvasTileData = new CanvasTileData
                {
                    ExtraData = submittedText
                }
            });
        }
    }
}
