using System;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
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
        return CanvasTileData.FullTileState();
    }

    public TileState TileStateWithMode(CanvasBrushMode canvasBrushMode)
    {
        var result = TileState();

        if (!canvasBrushMode.ForegroundShapeAndTransform.IsVisible)
        {
            result = result.WithSprite(ResourceAlias.Utility, 34);
        }

        if (!canvasBrushMode.ForegroundColor.IsVisible)
        {
            result = result with {ForegroundColor = Color.White};
        }

        if (!canvasBrushMode.BackgroundColorAndIntensity.IsVisible)
        {
            result = result with {BackgroundIntensity = 0};
        }

        return result;
    }
}
