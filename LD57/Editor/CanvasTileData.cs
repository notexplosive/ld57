using System;
using ExplogineMonoGame.AssetManagement;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace LD57.Editor;

[Serializable]
public record CanvasTileData
{
    [JsonProperty("background_color")]
    public string? BackgroundColorName;
    
    [JsonProperty("foreground_color")]
    public string? ForegroundColorName;
    
    [JsonProperty("sheet")]
    public string? SheetName;
    
    [JsonProperty("frame")]
    public int Frame;
    
    [JsonProperty("text")]
    public string? TextString;

    [JsonProperty("tile_type")]
    public TileType TileType;
    
    public TileState FullTileState()
    {
        if (TileType == TileType.Sprite)
        {
            var sheet = CalculateSheet();
            return TileState.Sprite(sheet, Frame, CalculateForegroundColor()).WithBackground(CalculateBackgroundColor());
        }

        if (TileType == TileType.Character)
        {
            return TileState.StringCharacter(TextString ?? "?", CalculateForegroundColor()).WithBackground(CalculateBackgroundColor());
        }

        return TileState.TransparentEmpty;
    }

    private SpriteSheet CalculateSheet()
    {
        return ResourceAlias.GetSpriteSheetByName(SheetName) ?? ResourceAlias.Entities;
    }

    private Color CalculateBackgroundColor()
    {
        return ResourceAlias.Color(BackgroundColorName);
    }

    private Color CalculateForegroundColor()
    {
        return ResourceAlias.Color(ForegroundColorName);
    }
}
