using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LD57.Gameplay;

[Serializable]
public class EntityTemplate
{
    [JsonProperty("sprite_sheet")]
    public string SpriteSheetName { get; set; } = "Entities";
    
    [JsonProperty("frame")]
    public int Frame { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; } = "ffffff";

    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new();
    
    [JsonProperty("sort_priority")]
    public int SortPriority { get; set; }

    [JsonProperty("state")]
    public Dictionary<string, string> State { get; set; } = new();

    public string TemplateName { get; set; } = string.Empty;
}
