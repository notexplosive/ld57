using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace LD57.Rendering;

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
