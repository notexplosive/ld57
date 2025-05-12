using System.Collections.Generic;
using System.Linq;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasEditorSelection : EditorSelection<PlacedCanvasTile>
{
    private readonly CanvasEditorSurface _surface;
    private readonly CanvasBrushMode _mode;

    public CanvasEditorSelection(CanvasEditorSurface surface, CanvasBrushMode mode)
    {
        _surface = surface;
        _mode = mode;
    }

    public override TileState GetTileStateAt(GridPosition internalPosition)
    {
        return (PlacedObjects.FirstOrDefault(a => a.Position == internalPosition)?.CanvasTileData.GetTileWithMode(_mode) ??
                TileState.BackgroundOnly(Color.White, 1f)) with
        {
            ForegroundColor = Color.DarkGoldenrod,
            BackgroundColor = Color.Goldenrod,
            BackgroundIntensity = 1f
        };
    }

    protected override IEnumerable<PlacedCanvasTile> GetAllObjectsAt(GridPosition position)
    {
        return _surface.AllItemsAt(position);
    }
}
