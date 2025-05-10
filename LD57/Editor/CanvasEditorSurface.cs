using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class CanvasEditorSurface : EditorSurface<CanvasData, PlacedCanvasTile, CanvasTileData>
{
    public CanvasEditorSurface() : base("Canvases")
    {
        RealSelection = new CanvasEditorSelection(this);
    }

    protected override CanvasEditorSelection RealSelection { get; }

    public override void PaintWorldToScreen(AsciiScreen screen, float dt)
    {
        foreach (var item in Data.Content)
        {
            screen.PutTile(item.Position, item.TileState());
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
}
