using System;
using System.Collections.Generic;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public readonly record struct GridRectangle
{
    public GridRectangle(GridPosition a, GridPosition b, bool isInclusive = false)
    {
        A = a;
        B = b;

        Left = Math.Min(a.X, b.X);
        Top = Math.Min(a.Y, b.Y);
        Width = Math.Abs(a.X - b.X);
        Height = Math.Abs(a.Y - b.Y);

        if (isInclusive)
        {
            Width++;
            Height++;
        }
    }

    public int Left { get; init; }
    public int Top { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Right => Left + Width - 1;
    public int Bottom => Top + Height - 1;

    public GridPosition TopLeft => new(Left, Top);
    public GridPosition BottomRight => new(Right, Bottom);
    public GridPosition Size => new(Width, Height);

    public GridPosition A { get; init; }
    public GridPosition B { get; init; }
    public GridPosition TopRight => new(Right, Top);
    public GridPosition BottomLeft => new(Left, Bottom);

    public static GridRectangle FromTopLeftAndSize(GridPosition topLeft, GridPosition size)
    {
        return new GridRectangle
        {
            Left = topLeft.X,
            Top = topLeft.Y,
            Width = size.X,
            Height = size.Y
        };
    }

    public Rectangle Rectangle(bool isInclusive)
    {
        var extra = new GridPosition(1, 1);

        if (!isInclusive)
        {
            extra = new GridPosition(0, 0);
        }

        return new Rectangle(TopLeft.ToPoint(),
            (BottomRight - TopLeft + extra).ToPoint());
    }

    public IEnumerable<GridPosition> AllPositions(bool isInclusive)
    {
        var extra = isInclusive ? 1 : 0;
        for (var x = TopLeft.X; x < BottomRight.X + extra; x++)
        {
            for (var y = TopLeft.Y; y < BottomRight.Y + extra; y++)
            {
                yield return new GridPosition(x, y);
            }
        }
    }

    public void Deconstruct(out GridPosition a, out GridPosition b)
    {
        a = A;
        b = B;
    }

    public GridRectangle Moved(GridPosition offset)
    {
        return new GridRectangle(A + offset, B + offset);
    }

    public bool Contains(GridPosition position, bool isInclusive)
    {
        return position.X >= Left && position.Y >= Top &&
               ((isInclusive && position.X < Right + 1 && position.Y < Bottom + 1) ||
                (!isInclusive && position.X < Right && position.Y < Bottom));
    }

    public static GridRectangle FromRectangleF(RectangleF rectangle)
    {
        return FromTopLeftAndSize(rectangle.Location.RoundToGridPosition(), rectangle.Size.RoundToGridPosition());
    }
}
