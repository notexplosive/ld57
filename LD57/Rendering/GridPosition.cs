using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

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

public readonly record struct GridPosition
{
    public GridPosition(Point point) : this(point.X, point.Y)
    {
    }

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    [JsonProperty("x")]
    public int X { get; init; }

    [JsonProperty("y")]
    public int Y { get; init; }

    public static GridPosition operator +(GridPosition a, GridPosition b)
    {
        return new GridPosition(a.X + b.X, a.Y + b.Y);
    }

    public static GridPosition operator -(GridPosition a, GridPosition b)
    {
        return new GridPosition(a.X - b.X, a.Y - b.Y);
    }

    public Point ToPoint()
    {
        return new Point(X, Y);
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}
