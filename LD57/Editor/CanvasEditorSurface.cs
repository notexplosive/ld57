using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class CanvasEditorSurface : EditorSurface<CanvasData, PlacedCanvasTile, CanvasTileData>
{
    private readonly CanvasBrushMode _canvasBrushMode;

    public CanvasEditorSurface(CanvasBrushMode canvasBrushMode) : base("Canvases", new CanvasData(canvasBrushMode))
    {
        _canvasBrushMode = canvasBrushMode;
        RealSelection = new CanvasEditorSelection(this, canvasBrushMode);
    }

    protected override CanvasEditorSelection RealSelection { get; }

    public override void PaintWorldToScreen(AsciiScreen screen, float dt)
    {
        foreach (var item in Data.Content)
        {
            screen.PutTile(item.Position, item.CanvasTileData.GetTileWithMode(_canvasBrushMode));
        }
    }

    public override void PaintOverlayBelowTool(AsciiScreen screen,
        GridPosition? hoveredWorldPosition)
    {
    }

    public override void PaintOverlayAboveTool(AsciiScreen screen)
    {
    }

    public override void Open(string path, bool isFullPath)
    {
    }

    public override void HandleKeyBinds(ConsumableInput input)
    {
    }

    protected override CanvasData CreateEmptyData()
    {
        return new CanvasData(_canvasBrushMode);
    }
}
