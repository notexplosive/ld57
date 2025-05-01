using System;
using System.Collections.Generic;
using System.Linq;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Gameplay;

[Serializable]
public class WorldTemplate
{
    [JsonProperty("entities")]
    public List<PlacedEntity> PlacedEntities = new();

    public void FillRectangle(GridPositionCorners rectangle, EntityTemplate template)
    {
        foreach (var position in rectangle.AllPositions(true))
        {
            PlaceTemplateAt(template, position);
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
            extraState.Add("unique_id", ((int) DateTimeOffset.Now.ToUnixTimeMilliseconds()).ToString());
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

    public void EraseRectangle(GridPositionCorners rectangle)
    {
        foreach (var position in rectangle.AllPositions(true))
        {
            RemoveEntitiesAtExceptMetadata(position);
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

    public void FillAllPositions(IEnumerable<GridPosition> positions, EntityTemplate template)
    {
        foreach (var position in positions)
        {
            PlaceTemplateAt(template, position);
        }
    }

    public void EraseAtPositions(IEnumerable<GridPosition> allPositions)
    {
        foreach (var position in allPositions)
        {
            EraseAt(position);
        }
    }

    private void EraseAt(GridPosition position)
    {
        RemoveEntitiesAtExceptMetadata(position);
    }

    private void PlaceTemplateAt(EntityTemplate template, GridPosition position)
    {
        RemoveEntitiesAtExceptMetadata(position);
        PlaceEntity(position, template);
    }
}
