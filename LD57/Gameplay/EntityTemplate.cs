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
    public string ColorHex { get; set; } = "ffffff";

    [JsonProperty("tags")]
    public List<string> Tags { get; set; } = new();
}
