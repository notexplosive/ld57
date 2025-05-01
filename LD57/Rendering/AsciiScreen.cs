using System;
using System.Collections;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public class AsciiScreen
{
    private readonly Lazy<DynamicSpriteFont> _font;
    private readonly Dictionary<GridPosition, TileState> _tiles = new();
    private readonly Dictionary<GridPosition, TweenableGlyph> _tweenableGlyphs = new();

    public AsciiScreen(int width, int height, float tileSize)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;
        Clear(TileState.TransparentEmpty);
        _font = new Lazy<DynamicSpriteFont>(() => ResourceAlias.GameFont.GetFont(tileSize * 0.8f));
    }

    public float TileSize { get; }

    public int Width { get; }
    public int Height { get; }

    public GridPosition RoomSize => new(Width - 1, Height - 1);
    public GridPosition CenterPosition => RoomSize / 2;

    public void Draw(Painter painter, Vector2 offset)
    {
        var tileRect = new Vector2(TileSize).ToRectangleF().Moved(offset);
        painter.BeginSpriteBatch();
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var rectangle = tileRect.Moved(new Vector2(TileSize * x, TileSize * y));
                var gridPosition = new GridPosition(x, y);
                var tileState = _tiles[gridPosition];
                var tweenableGlyph = _tweenableGlyphs.GetValueOrDefault(gridPosition) ?? new TweenableGlyph();
                var tweenBackgroundColor = tweenableGlyph.BackgroundColor.Value;

                var backgroundRectangle = rectangle.ScaledFromCenter(tileState.BackgroundIntensity);
                
                if (tweenBackgroundColor != Color.Transparent)
                {
                    painter.DrawRectangle(backgroundRectangle,
                        new DrawSettings {Depth = Depth.Back, Color = tweenBackgroundColor});
                }
                else if (tileState.HasBackground)
                {
                    painter.DrawRectangle(backgroundRectangle,
                        new DrawSettings {Depth = Depth.Back, Color = tileState.BackgroundColor});
                }

                var color = tileState.ForegroundColor;
                if (tweenableGlyph.ShouldOverrideColor)
                {
                    color = tweenableGlyph.ForegroundColorOverride;
                }

                var pixelOffset = tweenableGlyph.PixelOffset.Value;
                var rotation = tweenableGlyph.Rotation.Value;
                var scale = tweenableGlyph.Scale;

                if (tileState.TileType == TileType.Character)
                {
                    var text = tileState.Character;
                    if (!string.IsNullOrEmpty(text))
                    {
                        var measuredSize = _font.Value.MeasureString(text);
                        var origin = new Vector2(measuredSize.X / 2f, _font.Value.LineHeight * 4 / 6f);
                        // painter.DrawRectangle(measuredSize.ToRectangleF().Moved(rectangle.Center).Moved(-origin), new DrawSettings{Color = Color.White.WithMultipliedOpacity(0.5f)});
                        painter.SpriteBatch.DrawString(_font.Value, text, rectangle.Center + pixelOffset, color, rotation,
                            origin,
                            Vector2.One * scale);
                    }
                }
                else if (tileState.TileType == TileType.Sprite)
                {
                    if (tileState.SpriteSheet != null)
                    {
                        tileState.SpriteSheet.DrawFrameAsRectangle(painter, tileState.Frame,
                            rectangle.ScaledFromCenter(scale).MovedByOrigin(DrawOrigin.Center).Moved(pixelOffset),
                            new DrawSettings {Color = color, Origin = DrawOrigin.Center, Angle = rotation});
                    }
                }
            }
        }

        painter.EndSpriteBatch();
    }

    public void PutTile(GridPosition position, TileState tileState, TweenableGlyph? tweenableGlyph = null)
    {
        if (position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height)
        {
            _tiles[position] = tileState;
            if (tweenableGlyph != null)
            {
                _tweenableGlyphs[position] = tweenableGlyph;
            }
            else
            {
                _tweenableGlyphs.Remove(position);
            }
        }
    }

    public void PutSequence(GridPosition position, IEnumerable<TileState> tiles)
    {
        var index = 0;
        foreach(var tile in tiles)
        {
            PutTile(position + new GridPosition(index, 0), tile);
            index++;
        }
    }

    public void PutString(GridPosition position, string content, Color? color = null)
    {
        for (var index = 0; index < content.Length; index++)
        {
            var character = content.Substring(index, 1);
            PutTile(position + new GridPosition(index, 0), TileState.StringCharacter(character, color));
        }
    }

    public void PutFilledRectangle(TileState tileState, GridPosition cornerA, GridPosition cornerB)
    {
        var minX = Math.Min(cornerA.X, cornerB.X);
        var minY = Math.Min(cornerA.Y, cornerB.Y);
        var width = Math.Abs(cornerA.X - cornerB.X);
        var height = Math.Abs(cornerA.Y - cornerB.Y);

        var topLeft = new GridPosition(minX, minY);

        for (var x = 0; x < width+1; x++)
        {
            for (var y = 0; y < height+1; y++)
            {
                PutTile(topLeft + new GridPosition(x, y), tileState);
            }
        }
    }

    public void PutFrameRectangle(SpriteSheet frame, GridPosition cornerA, GridPosition cornerB)
    {
        var minX = Math.Min(cornerA.X, cornerB.X);
        var minY = Math.Min(cornerA.Y, cornerB.Y);
        var width = Math.Abs(cornerA.X - cornerB.X);
        var height = Math.Abs(cornerA.Y - cornerB.Y);

        var topLeft = new GridPosition(minX, minY);
        var bottomRight = new GridPosition(minX + width, minY + height);
        var bottomLeft = new GridPosition(minX, minY +height);
        var topRight = new GridPosition(minX+width, minY);

        PutTile(topLeft, TileState.Sprite(frame, 0));
        for (var i = 1; i < width; i++)
        {
            PutTile(topLeft + new GridPosition(i, 0), TileState.Sprite(frame, 1));
        }

        PutTile(topRight, TileState.Sprite(frame, 2));

        for (var i = 1; i < height; i++)
        {
            PutTile(topRight + new GridPosition(0, i), TileState.Sprite(frame, 3));
        }

        PutTile(bottomRight, TileState.Sprite(frame, 4));

        for (var i = 1; i < width; i++)
        {
            PutTile(bottomLeft + new GridPosition(i, 0), TileState.Sprite(frame, 5));
        }

        PutTile(bottomLeft, TileState.Sprite(frame, 6));

        for (var i = 1; i < height; i++)
        {
            PutTile(topLeft + new GridPosition(0, i), TileState.Sprite(frame, 7));
        }

        for (var x = 1; x < width; x++)
        {
            for (var y = 1; y < height; y++)
            {
                PutTile(topLeft + new GridPosition(x, y), TileState.TransparentEmpty);
            }
        }
    }

    public void Clear(TileState tileState)
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var gridPosition = new GridPosition(x, y);
                _tiles[gridPosition] = tileState;
                _tweenableGlyphs.Remove(gridPosition);
            }
        }
    }

    public GridPosition? GetHoveredTile(ConsumableInput input, HitTestStack hitTestStack, Vector2 offset)
    {
        var tileRect = new Vector2(TileSize).ToRectangleF().Moved(offset);

        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var position = new GridPosition(x, y);
                var rectangle = tileRect.Moved(new Vector2(TileSize * x, TileSize * y));
                if (rectangle.Contains(input.Mouse.Position(hitTestStack.WorldMatrix)))
                {
                    return position;
                }
            }
        }

        return null;
    }

    public TileState GetTile(GridPosition position)
    {
        return _tiles.GetValueOrDefault(position);
    }

    public IEnumerable<GridPosition> AllTiles()
    {
        return Constants.AllPositionsInRectangle(new GridPosition(0, 0), RoomSize);
    }

    public bool ContainsPosition(GridPosition screenPosition)
    {
        return screenPosition.X >= 0 && screenPosition.Y >= 0 && screenPosition.X < Width && screenPosition.Y < Height;
    }
}
