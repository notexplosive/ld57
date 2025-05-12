using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasEditorBrushTool : BrushTool
{
    private readonly CanvasEditorSurface _canvasEditorSurface;
    private readonly CanvasBrushMode _canvasBrushMode;

    public CanvasEditorBrushTool(EditorSession editorEditorSession, CanvasEditorSurface canvasEditorSurface,
        CanvasBrushMode canvasBrushMode) : base(editorEditorSession)
    {
        _canvasEditorSurface = canvasEditorSurface;
        _canvasBrushMode = canvasBrushMode;
    }

    public override TileState GetTileStateInWorldOnHover(TileState original)
    {
        return _canvasBrushMode.GetFullTile().GetTileWithMode(_canvasBrushMode);
    }

    protected override void OnErase(GridPosition hoveredWorldPosition)
    {
        _canvasEditorSurface.Data.EraseAt(hoveredWorldPosition);
    }

    protected override void OnPaint(GridPosition hoveredWorldPosition)
    {
        _canvasEditorSurface.Data.PlaceInkAt(hoveredWorldPosition, _canvasBrushMode.GetFullTile());
    }
}
