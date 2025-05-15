using System.Collections.Generic;
using System.Linq;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasEditorSelection : EditorSelection<PlacedCanvasTile>
{
    private readonly CanvasEditorSurface _surface;
    private readonly CanvasBrushFilter _filter;

    public CanvasEditorSelection(CanvasEditorSurface surface, CanvasBrushFilter filter)
    {
        _surface = surface;
        _filter = filter;
    }

    public override TileState GetTileStateAt(GridPosition internalPosition)
    {
        return (PlacedObjects.FirstOrDefault(a => a.Position == internalPosition)?.CanvasTileData.GetTileStateWithFilter(_filter, true) ??
                TileState.BackgroundOnly(Color.White, 1f)) with
        {
            ForegroundColor = Color.DarkGoldenrod,
            BackgroundColor = Color.Goldenrod,
            BackgroundIntensity = 1f
        };
    }

    protected override IEnumerable<PlacedCanvasTile> GetAllObjectsAt(GridPosition position)
    {
        return _surface.AllInkAt(position);
    }
}
