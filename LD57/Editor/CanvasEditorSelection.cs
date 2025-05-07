using System.Collections.Generic;
using System.Linq;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasEditorSelection : EditorSelection<PlacedCanvasTile>
{
    private readonly CanvasEditorSurface _surface;

    public CanvasEditorSelection(CanvasEditorSurface surface)
    {
        _surface = surface;
    }

    public override TileState GetTileStateAt(GridPosition internalPosition)
    {
        var entities = PlacedObjects.Where(a => a.Position == internalPosition);
        return entities.First().TileState();
    }

    protected override IEnumerable<PlacedCanvasTile> GetAllObjectsAt(GridPosition position)
    {
        return _surface.AllItemsAt(position);
    }
}
