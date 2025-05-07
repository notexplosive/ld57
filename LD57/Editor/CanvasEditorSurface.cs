using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class CanvasEditorSurface : EditorSurface<CanvasData, PlacedCanvasTile, CanvasTileData>
{
    private readonly CanvasEditorSelection _selection;

    public CanvasEditorSurface() : base("Canvases")
    {
        _selection = new CanvasEditorSelection(this);
    }

    protected override CanvasEditorSelection RealSelection => _selection;

    public override void PaintWorldToScreen(AsciiScreen screen, GridPosition cameraPosition, float dt)
    {
    }

    public override void PaintOverlayBelowTool(AsciiScreen screen, GridPosition cameraPosition,
        GridPosition? hoveredWorldPosition)
    {
    }

    public override void PaintOverlayAboveTool(AsciiScreen screen, GridPosition cameraPosition)
    {
    }

    public override void Open(string path, bool isFullPath)
    {
    }

    public override void HandleKeyBinds(ConsumableInput input)
    {
    }
}
