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
                RemoveEntitiesAtExceptMetadata(position);
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

        var extraState = new Dictionary<string, string>();

        if (template.Tags.Contains("Unique"))
        {
            extraState.Add("unique_id", ((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()).ToString());
        }

        PlacedEntities.Add(new PlacedEntity
        {
            Position = position,
            TemplateName = template.TemplateName,
            ExtraState = extraState
        });
    }

    public void RemoveEntitiesAt(GridPosition position)
    {
        PlacedEntities.RemoveAll(a => a.Position == position);
    }

    public void RemoveEntitiesAtExceptMetadata(GridPosition position)
    {
        PlacedEntities.RemoveAll(a => a.TemplateName != string.Empty && a.Position == position);
    }

    public IEnumerable<PlacedEntity> AllEntitiesAt(GridPosition position)
    {
        foreach (var entity in PlacedEntities)
        {
            if (entity.Position == position)
            {
                yield return entity;
            }
        }
    }

    public void EraseRectangle(Rectangle rectangle)
    {
        for (var x = rectangle.X; x < rectangle.X + rectangle.Width + 1; x++)
        {
            for (var y = rectangle.Y; y < rectangle.Y + rectangle.Height + 1; y++)
            {
                var position = new GridPosition(x, y);
                RemoveEntitiesAtExceptMetadata(position);
            }
        }
    }

    public void SetTile(GridPosition position, EntityTemplate template)
    {
        RemoveEntitiesAtExceptMetadata(position);
        PlaceEntity(position, template);
    }

    public void AddMetaEntity(GridPosition position, string command)
    {
        PlacedEntities.Add(new PlacedEntity
        {
            Position = position,
            TemplateName = string.Empty,
            ExtraState = new Dictionary<string, string>
            {
                {Constants.CommandKey, command}
            }
        });
    }

    public PlacedEntity? GetPlayerEntity()
    {
        return PlacedEntities.FirstOrDefault(a => a.TemplateName == "player");
    }

    public IEnumerable<PlacedEntity> GetMetadataAt(GridPosition position)
    {
        foreach (var entity in AllEntitiesAt(position))
        {
            if (entity.ExtraState.Count > 0)
            {
                yield return entity;
            }
        }
    }

    public void RemoveExactEntity(PlacedEntity entity)
    {
        PlacedEntities.Remove(entity);
    }

    public void AddExactEntity(PlacedEntity item)
    {
        PlacedEntities.Add(item);
    }
}
