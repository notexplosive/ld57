using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public class AsciiScreen
{
    private readonly DynamicSpriteFont _font = ResourceAlias.GameFont.GetFont(40);
    private readonly Dictionary<GridPosition, TileState> _tiles = new();
    private readonly Dictionary<GridPosition, TweenableGlyph> _tweenableGlyphs = new();

    public AsciiScreen(int width, int height, float tileSize)
    {
        Width = width;
        Height = height;
        TileSize = tileSize;
        Clear(TileState.Empty);
    }

    public float TileSize { get; }

    public int Width { get; }
    public int Height { get; }

    public GridPosition RoomSize => new(Width - 1, Height - 1);

    public void Draw(Painter painter)
    {
        var tileRect = new Vector2(TileSize).ToRectangleF().Moved(new Vector2(0, TileSize / 4f));
        painter.BeginSpriteBatch();
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var rectangle = tileRect.Moved(new Vector2(TileSize * x, TileSize * y));
                var gridPosition = new GridPosition(x, y);
                var tileState = _tiles[gridPosition];
                var tweenableGlyph = _tweenableGlyphs.GetValueOrDefault(gridPosition) ?? new TweenableGlyph();

                var color = tileState.Color;
                if (tweenableGlyph.ShouldOverrideColor)
                {
                    color = tweenableGlyph.ColorOverride;
                }
                
                var pixelOffset = tweenableGlyph.PixelOffset.Value;
                var rotation = tweenableGlyph.Rotation.Value;

                if (tileState.TileType == TileType.Character)
                {
                    var text = tileState.Character;
                    if (!string.IsNullOrEmpty(text))
                    {
                        var measuredSize = _font.MeasureString(text);
                        var origin = new Vector2(measuredSize.X / 2f, _font.LineHeight * 4 / 6f);
                        // painter.DrawRectangle(measuredSize.ToRectangleF().Moved(rectangle.Center).Moved(-origin), new DrawSettings{Color = Color.White.WithMultipliedOpacity(0.5f)});
                        painter.SpriteBatch.DrawString(_font, text, rectangle.Center + pixelOffset, color, rotation, origin,
                            Vector2.One);
                    }
                }
                else if (tileState.TileType == TileType.Sprite)
                {
                    if (tileState.SpriteSheet != null)
                    {
                        tileState.SpriteSheet.DrawFrameAsRectangle(painter, tileState.Frame,
                            rectangle.MovedByOrigin(DrawOrigin.Center).Moved(pixelOffset),
                            new DrawSettings {Color = color, Origin = DrawOrigin.Center, Angle = rotation});
                    }
                }
            }
        }

        painter.EndSpriteBatch();
    }

    public void SetTile(GridPosition position, TileState tileState, TweenableGlyph? tweenableGlyph = null)
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

    public void PutString(GridPosition position, string content, Color? color = null)
    {
        for (var index = 0; index < content.Length; index++)
        {
            var character = content.Substring(index, 1);
            SetTile(position + new GridPosition(index, 0), TileState.Glyph(character, color));
        }
    }

    public void PutRectangle(SpriteSheet frame, GridPosition topLeft, GridPosition bottomRight)
    {
        var bottomLeft = new GridPosition(topLeft.X, bottomRight.Y);
        var topRight = new GridPosition(bottomRight.X, topLeft.Y);

        var width = bottomRight.X - topLeft.X;
        var height = bottomRight.Y - topLeft.Y;

        SetTile(topLeft, TileState.Sprite(frame, 0));
        for (var i = 1; i < width; i++)
        {
            SetTile(topLeft + new GridPosition(i, 0), TileState.Sprite(frame, 1));
        }

        SetTile(topRight, TileState.Sprite(frame, 2));

        for (var i = 1; i < height; i++)
        {
            SetTile(topRight + new GridPosition(0, i), TileState.Sprite(frame, 3));
        }

        SetTile(bottomRight, TileState.Sprite(frame, 4));

        for (var i = 1; i < width; i++)
        {
            SetTile(bottomLeft + new GridPosition(i, 0), TileState.Sprite(frame, 5));
        }

        SetTile(bottomLeft, TileState.Sprite(frame, 6));

        for (var i = 1; i < height; i++)
        {
            SetTile(topLeft + new GridPosition(0, i), TileState.Sprite(frame, 7));
        }

        for (var x = 1; x < width; x++)
        {
            for (var y = 1; y < height; y++)
            {
                SetTile(topLeft + new GridPosition(x, y), TileState.Empty);
            }
        }
    }

    public void Clear(TileState tileState)
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                _tiles[new GridPosition(x, y)] = tileState;
            }
        }
    }
}
