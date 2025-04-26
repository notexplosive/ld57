using System;
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
    public int Width { get; }
    public int Height { get; }

    public GridPosition TopLeft => new(Left, Top);
    public GridPosition BottomRight => new(Left + Width, Top + Height);

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

    public void Deconstruct(out GridPosition A, out GridPosition B)
    {
        A = this.A;
        B = this.B;
    }
}
