namespace LD57.Rendering;

public readonly record struct GridPosition(int X, int Y)
{
    public static GridPosition operator +(GridPosition a, GridPosition b)
    {
        return new GridPosition(a.X + b.X, a.Y + b.Y);
    }
}
