using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public class AsciiScreen
{
    private readonly DynamicSpriteFont _font;
    private readonly Dictionary<GridPosition, TileState> _tiles;
    private readonly float _tileSize;

    public AsciiScreen(int width, int height, float tileSize)
    {
        Width = width;
        Height = height;
        _tileSize = tileSize;
        _tiles = new Dictionary<GridPosition, TileState>();
        _font = ResourceAlias.GameFont.GetFont(40);

        Clear(TileState.Empty);
    }

    public int Width { get; }
    public int Height { get; }

    public GridPosition RoomSize => new(Width - 1, Height - 1);

    public void Draw(Painter painter)
    {
        var tileRect = new Vector2(_tileSize).ToRectangleF().Moved(new Vector2(0, _tileSize / 4f));
        painter.BeginSpriteBatch();
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var rectangle = tileRect.Moved(new Vector2(_tileSize * x, _tileSize * y));
                var tileState = _tiles[new GridPosition(x, y)];

                if (tileState.TileType == TileType.Character)
                {
                    var text = tileState.Character;
                    if (!string.IsNullOrEmpty(text))
                    {
                        var measuredSize = _font.MeasureString(text);
                        var origin = new Vector2(measuredSize.X / 2f, _font.LineHeight * 4 / 6f);
                        // painter.DrawRectangle(measuredSize.ToRectangleF().Moved(rectangle.Center).Moved(-origin), new DrawSettings{Color = Color.White.WithMultipliedOpacity(0.5f)});
                        painter.SpriteBatch.DrawString(_font, text, rectangle.Center, tileState.Color, 0, origin,
                            Vector2.One);
                    }
                }
                else if (tileState.TileType == TileType.Sprite)
                {
                    if (tileState.SpriteSheet != null)
                    {
                        tileState.SpriteSheet.DrawFrameAsRectangle(painter, tileState.Frame, rectangle,
                            new DrawSettings {Color = tileState.Color});
                    }
                }
            }
        }

        painter.EndSpriteBatch();
    }

    public void SetTile(GridPosition position, TileState tileState)
    {
        if (position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height)
        {
            _tiles[position] = tileState;
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

    public void PutRectangle(SpriteSheet popupFrame, GridPosition topLeft, GridPosition bottomRight)
    {
        var bottomLeft = new GridPosition(topLeft.X, bottomRight.Y);
        var topRight = new GridPosition(bottomRight.X, topLeft.Y);

        var width = bottomRight.X - topLeft.X;
        var height = bottomRight.Y - topLeft.Y;

        SetTile(topLeft, TileState.Sprite(popupFrame, 0));
        for (var i = 1; i < width; i++)
        {
            SetTile(topLeft + new GridPosition(i, 0), TileState.Sprite(popupFrame, 1));
        }

        SetTile(topRight, TileState.Sprite(popupFrame, 2));

        for (var i = 1; i < height; i++)
        {
            SetTile(topRight + new GridPosition(0, i), TileState.Sprite(popupFrame, 3));
        }

        SetTile(bottomRight, TileState.Sprite(popupFrame, 4));

        for (var i = 1; i < width; i++)
        {
            SetTile(bottomLeft + new GridPosition(i, 0), TileState.Sprite(popupFrame, 5));
        }

        SetTile(bottomLeft, TileState.Sprite(popupFrame, 6));

        for (var i = 1; i < height; i++)
        {
            SetTile(topLeft + new GridPosition(0, i), TileState.Sprite(popupFrame, 7));
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
