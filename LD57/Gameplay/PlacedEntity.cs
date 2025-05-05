using System;
using System.Collections.Generic;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Gameplay;

public interface IPlacedObject<out TPlaced> where TPlaced : IPlacedObject<TPlaced>
{
    public GridPosition Position { get; set; }
    public TPlaced MovedBy(GridPosition offset);
}

[Serializable]
public class PlacedEntity : IPlacedObject<PlacedEntity>
{
    [JsonProperty("extra_state")]
    public Dictionary<string, string> ExtraState = new();

    [JsonProperty("template_name")]
    public string TemplateName { get; set; } = string.Empty;

    [JsonProperty("position")]
    public GridPosition Position { get; set; }

    public PlacedEntity MovedBy(GridPosition offset)
    {
        return new PlacedEntity
        {
            TemplateName = TemplateName,
            Position = Position + offset,
            ExtraState = new Dictionary<string, string>(ExtraState)
        };
    }
}
