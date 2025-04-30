using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public readonly record struct GridPositionCorners
{
    public GridPositionCorners(GridPosition a, GridPosition b)
    {
        A = a;
        B = b;

        Left = Math.Min(a.X, b.X);
        Top = Math.Min(a.Y, b.Y);
        Width = Math.Abs(a.X - b.X);
        Height = Math.Abs(a.Y - b.Y);
    }

    public int Left { get; }
    public int Top { get; }
    public int Right => Left + Width;
    public int Bottom => Top + Height;
    public int Width { get; }
    public int Height { get; }

    public GridPosition TopLeft => new(Left, Top);
    public GridPosition BottomRight => new(Left + Width, Top + Height);

    public GridPosition Size => new(Width, Height);

    public GridPosition A { get; init; }
    public GridPosition B { get; init; }

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

    public GridPositionCorners Moved(GridPosition offset)
    {
        return new GridPositionCorners(A + offset, B + offset);
    }

    public bool Contains(GridPosition position, bool isInclusive)
    {
        return position.X >= Left && position.Y >= Top &&
               ((isInclusive && position.X < Right + 1 && position.Y < Bottom + 1) ||
                (!isInclusive && position.X < Right && position.Y < Bottom));
    }
}
