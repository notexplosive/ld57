using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public class CanvasEditorSurface : EditorSurface<CanvasData, PlacedCanvasTile, CanvasTileData, CanvasBrushFilter>
{
    private readonly CanvasBrushFilter _canvasBrushFilter;
    private readonly List<KeybindChord> _chords;

    public CanvasEditorSurface(CanvasBrushFilter canvasBrushFilter, List<KeybindChord> chords) : base("Canvases", new CanvasData())
    {
        _canvasBrushFilter = canvasBrushFilter;
        _chords = chords;
        RealSelection = new CanvasEditorSelection(this, canvasBrushFilter);
    }

    protected override CanvasEditorSelection RealSelection { get; }

    public override void PaintWorldToScreen(AsciiScreen screen, float dt)
    {
        foreach (var item in Data.Content)
        {
            screen.PutTile(item.Position, item.CanvasTileData.GetTileStateWithFilter(_canvasBrushFilter, true));
        }
    }

    public override void PaintOverlayBelowTool(AsciiScreen screen,
        GridPosition? hoveredWorldPosition)
    {
    }

    public override void PaintOverlayAboveTool(AsciiScreen screen)
    {
    }

    public override void HandleKeyBinds(ConsumableInput input)
    {
        foreach (var chord in _chords)
        {
            chord.ListenForFirstKey(input, this);
        }
    }

    protected override CanvasData CreateEmptyData()
    {
        return new CanvasData();
    }
}
