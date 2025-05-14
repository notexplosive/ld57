using System.Linq;
using LD57.Editor;
using LD57.Rendering;

namespace LD57.CartridgeManagement;

public class CanvasEyeDropperTool : EyeDropperTool
{
    private readonly CanvasEditorSurface _canvasSurface;
    private readonly CanvasBrushFilter _filter;

    public CanvasEyeDropperTool(EditorSession editorSession, CanvasEditorSurface canvasSurface, CanvasBrushFilter filter) : base(editorSession)
    {
        _canvasSurface = canvasSurface;
        _filter = filter;
    }

    protected override string DescribeTileAt(GridPosition position)
    {
        var ink = _canvasSurface.AllInkAt(position).FirstOrDefault();

        if (ink != null)
        {
            return ink.CanvasTileData.ToString();
        }

        return "";
    }

    protected override void GrabTile(GridPosition position)
    {
        var ink = _canvasSurface.AllInkAt(position).FirstOrDefault();
        if (ink != null)
        {
            _filter.SetBasedOnData(ink.CanvasTileData);
        }
    }
}
