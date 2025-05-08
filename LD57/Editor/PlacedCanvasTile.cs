using System;
using LD57.Gameplay;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Editor;

[Serializable]
public record PlacedCanvasTile : IPlacedObject<PlacedCanvasTile>
{
    [JsonProperty("data")]
    public CanvasTileData CanvasTileData { get; set; } = new();

    [JsonProperty("position")]
    public GridPosition Position { get; set; }

    public PlacedCanvasTile MovedBy(GridPosition offset)
    {
        return this with
        {
            Position = Position + offset
        };
    }

    public TileState TileState()
    {
        return Rendering.TileState.StringCharacter("#");
    }
}