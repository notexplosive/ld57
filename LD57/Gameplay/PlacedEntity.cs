using System;
using System.Collections.Generic;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Gameplay;

[Serializable]
public class PlacedEntity
{
    [JsonProperty("template_name")]
    public string TemplateName { get; set; } = string.Empty;

    [JsonProperty("position")]
    public GridPosition Position { get; set; }
    
    [JsonProperty("extra_data")]
    public Dictionary<string, string> ExtraData = new(); 
}
