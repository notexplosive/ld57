using System;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Editor;

[Serializable]
public class CanvasTile
{
    [JsonProperty("background_color")]
    public string? BackgroundColorName { get; set; }

    [JsonProperty("background_intensity")]
    public float BackgroundIntensity { get; set; }

    [JsonProperty("character")]
    public string? Character { get; set; }

    [JsonProperty("foreground_color")]
    public string? ForegroundColorName { get; set; }

    [JsonProperty("frame")]
    public int Frame { get; set; }

    [JsonProperty("sheet")]
    public string? SpriteSheet { get; set; }

    [JsonProperty("tile_type")]
    public TileType TileType { get; set; }
}
