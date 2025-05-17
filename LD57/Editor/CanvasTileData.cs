using System;
using System.Text;
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
    public string? ForegroundColorName = "default";

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

    public TileState GetTile()
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

        return result;
    }

    public TileState GetTileStateWithFilter(CanvasBrushFilter filter, bool isForVisibility)
    {
        var result = GetTile();

        if (!filter.ForegroundShapeAndTransform.Check(isForVisibility))
        {
            result = result.WithSprite(ResourceAlias.Utility, 34);
        }

        if (!filter.ForegroundColor.Check(isForVisibility))
        {
            result = result with {ForegroundColor = ResourceAlias.Color("default")};
        }

        if (!filter.BackgroundColorAndIntensity.Check(isForVisibility))
        {
            result = result with {BackgroundIntensity = 0};
        }

        return result;
    }

    public CanvasTileData WithShapeData(TileType tileType, string? sheetName, int frame, string? text, bool flipX,
        bool flipY, float angle)
    {
        return this with
        {
            TileType = tileType,
            FlipX = flipX,
            FlipY = flipY,
            Angle = angle,
            SheetName = sheetName,
            Frame = frame,
            TextString = text
        };
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        // shows up in Eye Dropper Tool Status
        if (TileType == TileType.Sprite)
        {
            stringBuilder.Append($"Sprite: {SheetName}[{Frame}] ");
            
            // only sprites can be flipped for reasons I don't understand
            stringBuilder.Append($"{(FlipX ? "FlipX, " : "")}{(FlipY ? "FlipY" : "")} ");
        }

        if (TileType == TileType.Character)
        {
            stringBuilder.Append($"Text: {TextString} ");
            
        }

        if (TileType != TileType.Skip)
        {
            if (Angle != 0)
            {
                stringBuilder.Append($"Rot: {Angle} ");
            }

            stringBuilder.Append($"Fg: {ForegroundColorName} ");
        }

        if (BackgroundIntensity > 0 && BackgroundColorName != null)
        {
            stringBuilder.Append($"Bg: {BackgroundColorName}@{BackgroundIntensity}");
        }

        return stringBuilder.ToString();
    }

    public ICanvasTileShape GetShape()
    {
        return TileType switch
        {
            TileType.Character => new CanvasTileShapeString(TextString ?? "?"),
            TileType.Sprite => new CanvasTileShapeSprite(SheetName ?? "Entities", Frame),
            _ => new CanvasTileShapeEmpty()
        };
    }
}
