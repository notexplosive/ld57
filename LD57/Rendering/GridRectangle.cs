using System;
using System.Collections.Generic;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public readonly record struct GridRectangle(int Left, int Top, int Width, int Height)
{
    public GridRectangle(GridPosition cornerA, GridPosition cornerB)
        : this(Math.Min(cornerA.X, cornerB.X),
            Math.Min(cornerA.Y, cornerB.Y),
            Math.Abs(cornerA.X - cornerB.X),
            Math.Abs(cornerA.Y - cornerB.Y))
    {
    }

    public int Right => Left + Width;
    public int Bottom => Top + Height;

    public GridPosition TopLeft => new(Left, Top);
    public GridPosition BottomRight => new(Right, Bottom);
    public GridPosition Size => new(Width, Height);

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

    public IEnumerable<GridPosition> AllPositions()
    {
        var extra = 1;
        for (var x = TopLeft.X; x < BottomRight.X + extra; x++)
        {
            for (var y = TopLeft.Y; y < BottomRight.Y + extra; y++)
            {
                yield return new GridPosition(x, y);
            }
        }
    }

    public GridRectangle Moved(GridPosition offset)
    {
        return new GridRectangle(TopLeft + offset, BottomRight + offset);
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
