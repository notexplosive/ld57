using System;
using System.Collections.Generic;
using System.Linq;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace LD57.Gameplay;

[Serializable]
public class WorldTemplate
{
    [JsonProperty("entities")]
    public List<PlacedEntity> PlacedEntities = new();

    public void FillRectangle(Rectangle rectangle, EntityTemplate template)
    {
        for (var x = rectangle.X; x < rectangle.X + rectangle.Width + 1; x++)
        {
            for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height + 1; y++)
            {
                var position = new GridPosition(x, y);
                RemoveEntitiesAt(position);
                PlaceEntity(position, template);
            }
        }
    }

    private void PlaceEntity(GridPosition position, EntityTemplate template)
    {
        if (template.TemplateName == "player")
        {
            PlacedEntities.RemoveAll(a => a.TemplateName == "player");
        }
        
        PlacedEntities.Add(new PlacedEntity
        {
            Position = position,
            TemplateName = template.TemplateName
        });
    }

    public void RemoveEntitiesAt(GridPosition position)
    {
        PlacedEntities.RemoveAll(a => a.Position == position);
    }

    public void EraseRectangle(Rectangle rectangle)
    {
        for (var x = rectangle.X; x < rectangle.X + rectangle.Width + 1; x++)
        {
            for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height + 1; y++)
            {
                var position = new GridPosition(x, y);
                RemoveEntitiesAt(position);
            }
        }
    }

    public void SetTile(GridPosition position, EntityTemplate template)
    {
        RemoveEntitiesAt(position);
        PlaceEntity(position, template);
    }

    public PlacedEntity? GetPlayerEntity()
    {
        return PlacedEntities.FirstOrDefault(a => a.TemplateName == "player");
    }
}
