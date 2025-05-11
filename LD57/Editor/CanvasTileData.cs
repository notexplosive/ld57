using System;
using ExplogineCore.Data;
using ExplogineMonoGame.AssetManagement;
using LD57.Core;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace LD57.Editor;

[Serializable]
public record CanvasTileData
{
    [JsonProperty("background_color")]
    public string? BackgroundColorName;

    [JsonProperty("flip_x")]
    public bool FlipX;

    [JsonProperty("flip_y")]
    public bool FlipY;

    [JsonProperty("foreground_color")]
    public string? ForegroundColorName;

    [JsonProperty("frame")]
    public int Frame;

    [JsonProperty("sheet")]
    public string? SheetName;

    [JsonProperty("text")]
    public string? TextString;

    [JsonProperty("tile_type")]
    public TileType TileType;

    [JsonProperty("rotation")]
    public float Angle { get; set; }

    [JsonProperty("background_intensity")]
    public float BackgroundIntensity { get; set; }

    public TileState FullTileState()
    {
        if (TileType == TileType.Sprite)
        {
            var sheet = CalculateSheet();
            return TileState.Sprite(sheet, Frame, CalculateForegroundColor())
                    .WithBackground(CalculateBackgroundColor(), BackgroundIntensity) with
                {
                    Flip = GetFlipState(),
                    Angle = Angle
                };
        }

        if (TileType == TileType.Character)
        {
            return TileState.StringCharacter(TextString ?? "?", CalculateForegroundColor())
                    .WithBackground(CalculateBackgroundColor(), BackgroundIntensity) with
                {
                    Flip = GetFlipState(),
                    Angle = Angle
                };
        }

        if (TileType == TileType.Invisible)
        {
            return TileState.BackgroundOnly(CalculateBackgroundColor(), BackgroundIntensity);
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

    public static CanvasTileData FromSettings(ICanvasTileShape currentShape, XyBool flipState, QuarterRotation rotation,
        string foregroundColor, string backgroundColor, float backgroundIntensity)
    {
        var result = new CanvasTileData();

        var tileStateFromShape = currentShape.GetTileState();

        result.TileType = tileStateFromShape.TileType;
        result.FlipX = flipState.X;
        result.FlipY = flipState.Y;
        result.Angle = rotation.Radians;
        result.ForegroundColorName = foregroundColor;
        result.BackgroundIntensity = backgroundIntensity;
        result.BackgroundColorName = backgroundColor;

        if (currentShape is CanvasTileShapeSprite spriteShape)
        {
            result.SheetName = spriteShape.SheetName;
            result.Frame = spriteShape.Frame;
        }

        if (currentShape is CanvasTileShapeString stringShape)
        {
            result.TextString = stringShape.StringContent;
        }

        // todo: take influence from other settings (eg: color)

        return result;
    }
}
