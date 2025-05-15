using System.Linq;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasEditorBrushTool : BrushTool
{
    private readonly CanvasEditorSurface _canvasEditorSurface;
    private readonly CanvasBrushFilter _canvasBrushFilter;

    public CanvasEditorBrushTool(EditorSession editorEditorSession, CanvasEditorSurface canvasEditorSurface,
        CanvasBrushFilter canvasBrushFilter) : base(editorEditorSession)
    {
        _canvasEditorSurface = canvasEditorSurface;
        _canvasBrushFilter = canvasBrushFilter;
    }

    public override TileState GetTileStateInWorldOnHover(TileState original)
    {
        return _canvasBrushFilter.GetFullTile().GetTileStateWithFilter(_canvasBrushFilter, false);
    }

    protected override void OnErase(GridPosition hoveredWorldPosition)
    {
        _canvasEditorSurface.Data.EraseAt(hoveredWorldPosition);
    }

    protected override void OnPaint(GridPosition hoveredWorldPosition)
    {
        _canvasEditorSurface.Data.PlaceInkAt(hoveredWorldPosition, _canvasBrushFilter.GetFullTile(), _canvasBrushFilter);
    }
}
