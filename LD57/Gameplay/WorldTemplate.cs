using System;
using System.Collections.Generic;
using System.Linq;
using LD57.Editor;
using LD57.Rendering;

namespace LD57.Gameplay;

[Serializable]
public class WorldTemplate : EditorData<PlacedEntity, EntityTemplate>
{
    public void FillRectangle(GridPositionCorners rectangle, EntityTemplate template)
    {
        foreach (var position in rectangle.AllPositions(true))
        {
            PlaceInkAt(position, template);
        }
    }

    private void PlaceEntity(GridPosition position, EntityTemplate template)
    {
        if (template.TemplateName == "player")
        {
            Content.RemoveAll(a => a.TemplateName == "player");
        }

        var extraState = new Dictionary<string, string>();

        if (template.Tags.Contains("Unique"))
        {
            extraState.Add("unique_id", ((int) DateTimeOffset.Now.ToUnixTimeMilliseconds()).ToString());
        }

        Content.Add(new PlacedEntity
        {
            Position = position,
            TemplateName = template.TemplateName,
            ExtraState = extraState
        });
    }

    public void RemoveEntitiesAtExceptMetadata(GridPosition position)
    {
        Content.RemoveAll(a => a.TemplateName != string.Empty && a.Position == position);
    }
    
    public override void PlaceInkAt(GridPosition position, EntityTemplate template)
    {
        RemoveEntitiesAtExceptMetadata(position);
        PlaceEntity(position, template);
    }

    public void AddMetaEntity(GridPosition position, string command)
    {
        Content.Add(new PlacedEntity
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
        return Content.FirstOrDefault(a => a.TemplateName == "player");
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

    public override void EraseAt(GridPosition position)
    {
        RemoveEntitiesAtExceptMetadata(position);
    }
}
