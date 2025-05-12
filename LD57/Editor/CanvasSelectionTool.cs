using System.Collections.Generic;
using LD57.Rendering;

namespace LD57.Editor;

public class CanvasSelectionTool : SelectionTool
{
    private readonly CanvasBrushFilter _canvasBrushFilter;

    public CanvasSelectionTool(EditorSession editorSession, CanvasEditorSurface surface,
        CanvasBrushFilter canvasBrushFilter) : base(editorSession)
    {
        _canvasBrushFilter = canvasBrushFilter;
        Surface = surface;
    }

    protected override CanvasEditorSurface Surface { get; }

    protected override void FillWithCurrentInk(List<GridPosition> positions)
    {
        Surface.Data.FillAllPositions(positions, _canvasBrushFilter.GetFullTile());
    }
}
