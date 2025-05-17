using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
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
    private readonly Stack<GridPosition> _transforms = new();
    private readonly Dictionary<GridPosition, TweenableGlyph> _tweenableGlyphs = new();
    private readonly Stack<GridRectangle> _stencils = new();

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
    public GridRectangle RoomRectangle => new(new GridPosition(0, 0), RoomSize);

    public void Draw(Painter painter, Vector2 wholeScreenPixelOffset)
    {
        var tileRect = new Vector2(TileSize).ToRectangleF().Moved(wholeScreenPixelOffset);
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
                var rotation = tweenableGlyph.Rotation.Value + tileState.Angle;
                var scale = tweenableGlyph.Scale;

                if (tileState.TileType == TileType.Character)
                {
                    var text = tileState.Character;
                    if (!string.IsNullOrEmpty(text))
                    {
                        var measuredSize = _font.Value.MeasureString(text);
                        var origin = new Vector2(measuredSize.X / 2f, _font.Value.LineHeight * 4 / 6f);
                        // painter.DrawRectangle(measuredSize.ToRectangleF().Moved(rectangle.Center).Moved(-origin), new DrawSettings{Color = Color.White.WithMultipliedOpacity(0.5f)});
                        painter.SpriteBatch.DrawString(_font.Value, text, rectangle.Center + pixelOffset, color,
                            rotation,
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
                            new DrawSettings
                                {Color = color, Origin = DrawOrigin.Center, Angle = rotation, Flip = tileState.Flip});
                    }
                }
            }
        }

        painter.EndSpriteBatch();
    }

    public void PutTile(GridPosition rawPosition, TileState tileState, TweenableGlyph? tweenableGlyph = null)
    {
        if (ContainsPosition(rawPosition + CurrentTransform()))
        {
            var transformedPosition = rawPosition + CurrentTransform();
            _tiles[transformedPosition] = tileState;
            if (tweenableGlyph != null)
            {
                _tweenableGlyphs[transformedPosition] = tweenableGlyph;
            }
            else
            {
                _tweenableGlyphs.Remove(transformedPosition);
            }
        }
    }

    private GridPosition CurrentTransform()
    {
        if (_transforms.TryPeek(out var result))
        {
            return result;
        }

        return GridPosition.Zero;
    }

    public void PutSequence(GridPosition position, IEnumerable<TileState> tiles)
    {
        var index = 0;
        foreach (var tile in tiles)
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

    public void PutFilledRectangle(TileState tileState, GridRectangle rectangle)
    {
        foreach (var position in rectangle.AllPositions())
        {
            PutTile(position, tileState);
        }
    }

    public void PutFilledRectangle(TileState tileState, GridPosition cornerA, GridPosition cornerB)
    {
        PutFilledRectangle(tileState, new GridRectangle(cornerA, cornerB));
    }

    public void PutFrameRectangle(SpriteSheet sheet, GridRectangle rectangle)
    {
        for (var i = 1; i < rectangle.Width; i++)
        {
            PutTile(rectangle.TopLeft + new GridPosition(i, 0), TileState.Sprite(sheet, 1));
        }

        for (var i = 1; i < rectangle.Height; i++)
        {
            PutTile(rectangle.TopRight + new GridPosition(0, i), TileState.Sprite(sheet, 3));
        }

        for (var i = 1; i < rectangle.Width; i++)
        {
            PutTile(rectangle.BottomLeft + new GridPosition(i, 0), TileState.Sprite(sheet, 5));
        }

        for (var i = 1; i < rectangle.Height; i++)
        {
            PutTile(rectangle.TopLeft + new GridPosition(0, i), TileState.Sprite(sheet, 7));
        }

        PutTile(rectangle.TopLeft, TileState.Sprite(sheet, 0));
        PutTile(rectangle.TopRight, TileState.Sprite(sheet, 2));
        PutTile(rectangle.BottomRight, TileState.Sprite(sheet, 4));
        PutTile(rectangle.BottomLeft, TileState.Sprite(sheet, 6));

        for (var x = 1; x < rectangle.Width; x++)
        {
            for (var y = 1; y < rectangle.Height; y++)
            {
                PutTile(rectangle.TopLeft + new GridPosition(x, y), TileState.TransparentEmpty);
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
                    return position + CurrentTransform();
                }
            }
        }

        return null;
    }

    public TileState GetTile(GridPosition position)
    {
        return _tiles.GetValueOrDefault(position + CurrentTransform());
    }

    public IEnumerable<GridPosition> AllTiles()
    {
        return Constants.AllPositionsInRectangle(CurrentTransform(), RoomSize);
    }

    public bool ContainsPosition(GridPosition transformedPosition)
    {
        if (_stencils.Count == 0)
        {
            return new GridRectangle(0, 0, Width, Height).Contains(transformedPosition, true);
        }

        foreach (var stencil in _stencils)
        {
            if (!stencil.Contains(transformedPosition, true))
            {
                return false;
            }
        }
        return true;
    }


    public void PushTransform(GridPosition offset)
    {
        _transforms.Push(CurrentTransform() + offset);
    }

    public void PopTransform()
    {
        if (!_transforms.TryPop(out _))
        {
            Client.Debug.LogError("Popped Screen Transform when there was none to pop");
        }
    }

    /// <summary>
    /// Pushes a stencil using the current Transform
    /// </summary>
    public void PushStencil(GridRectangle stencil)
    {
        _stencils.Push(stencil.Moved(CurrentTransform()));
    }

    public void PopStencil()
    {
        if (!_stencils.TryPop(out _))
        {
            Client.Debug.LogError("Popped Screen Stencil when there was none to pop");
        }
    }
}
