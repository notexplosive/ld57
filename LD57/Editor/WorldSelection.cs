using System.Collections.Generic;
using System.Linq;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class WorldSelection : EditorSelection<PlacedEntity>
{
    private readonly WorldEditorSurface _surface;

    public WorldSelection(WorldEditorSurface surface)
    {
        _surface = surface;
    }

    protected override IEnumerable<PlacedEntity> GetAllObjectsAt(GridPosition position)
    {
        return _surface.WorldTemplate.AllEntitiesAt(position);
    }

    public override TileState GetTileState(GridPosition internalPosition)
    {
        var entities = PlacedObjects.Where(a => a.Position == internalPosition);
        var topTemplate = entities
            .Select(entity => ResourceAlias.EntityTemplate(entity.TemplateName) ?? new EntityTemplate())
            .OrderBy(a => a.SortPriority).FirstOrDefault();

        if (topTemplate == null)
        {
            return TileState.BackgroundOnly(Color.Goldenrod, 1f);
        }

        return topTemplate.CreateAppearance().TileState with
        {
            ForegroundColor = Color.DarkGoldenrod,
            BackgroundColor = Color.Goldenrod,
            BackgroundIntensity = 1f
        };
    }
}
