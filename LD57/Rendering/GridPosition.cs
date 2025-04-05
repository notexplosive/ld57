using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public readonly record struct GridPosition(int X, int Y)
{
    public GridPosition(Point point) : this(point.X, point.Y)
    {
    }

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
}
