using System.Collections.Generic;
using LD57.Rendering;

namespace LD57.Editor;

public class CanvasSelectionTool : SelectionTool
{
    private readonly CanvasBrushMode _canvasBrushMode;

    public CanvasSelectionTool(EditorSession editorSession, CanvasEditorSurface surface,
        CanvasBrushMode canvasBrushMode) : base(editorSession)
    {
        _canvasBrushMode = canvasBrushMode;
        Surface = surface;
    }

    protected override CanvasEditorSurface Surface { get; }

    protected override void FillWithCurrentInk(List<GridPosition> positions)
    {
        Surface.Data.FillAllPositions(positions, _canvasBrushMode.GetTile());
    }
}
