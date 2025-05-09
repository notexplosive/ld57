using System;
using ExplogineCore.Data;
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

    [JsonProperty("flip_x")]
    public bool FlipX;
    
    [JsonProperty("flip_y")]
    public bool FlipY;
    
    public TileState FullTileState()
    {
        if (TileType == TileType.Sprite)
        {
            var sheet = CalculateSheet();
            return TileState.Sprite(sheet, Frame, CalculateForegroundColor()).WithBackground(CalculateBackgroundColor() , 0.25f) with {Flip = GetFlipState()};
        }

        if (TileType == TileType.Character)
        {
            return TileState.StringCharacter(TextString ?? "?", CalculateForegroundColor()).WithBackground(CalculateBackgroundColor()) with {Flip = GetFlipState()};
        }

        return TileState.TransparentEmpty;
    }

    private XyBool GetFlipState()
    {
        return new XyBool(FlipX, FlipY);
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

    public static CanvasTileData FromSettings(ICanvasTileShape currentShape, XyBool flipState)
    {
        var result = new CanvasTileData();

        var tileStateFromShape = currentShape.GetTileState();

        result.TileType = tileStateFromShape.TileType;
        result.FlipX = flipState.X;
        result.FlipY = flipState.Y;

        if (currentShape is CanvasTileShapeSprite spriteShape)
        {
            result.SheetName = spriteShape.SheetName;
            result.Frame = spriteShape.Frame;
        }

        if (currentShape is CanvasTileShapeString stringShape)
        {
            result.TextString = stringShape.StringContent;
        }

        
        // todo: take influence from other settings
        
        return result;
    }
}
