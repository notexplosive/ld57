using System;

namespace LD57.Core;

public class QuarterRotation
{
    public static readonly QuarterRotation Upright = new(0);
    public static readonly QuarterRotation Quarter = new(90);
    public static readonly QuarterRotation Half = new(180);
    public static readonly QuarterRotation ThreeQuarters = new(270);

    private QuarterRotation(float degrees)
    {
        Degrees = degrees;
    }

    public float Degrees { get; }
    public float Radians => Degrees / 360f * MathF.PI * 2;

    public QuarterRotation ClockwiseNext()
    {
        if (this == Upright)
        {
            return Quarter;
        }

        if (this == Quarter)
        {
            return Half;
        }

        if (this == Half)
        {
            return ThreeQuarters;
        }

        if (this == ThreeQuarters)
        {
            return Upright;
        }

        // Should never happen
        return this;
    }

    public QuarterRotation CounterClockwisePrevious()
    {
        if (this == Upright)
        {
            return ThreeQuarters;
        }

        if (this == Quarter)
        {
            return Upright;
        }

        if (this == Half)
        {
            return Quarter;
        }

        if (this == ThreeQuarters)
        {
            return Half;
        }

        // Should never happen
        return this;
    }

    public static QuarterRotation FromAngleDegrees(float angle)
    {
        // insanely forgiving tolerance
        var tolerance = 45;
        if (Math.Abs(angle - 0f) < tolerance)
        {
            return Upright;
        }

        if (Math.Abs(angle - 90) < tolerance)
        {
            return Quarter;
        }

        if (Math.Abs(angle - 180) < tolerance)
        {
            return Half;
        }

        if (Math.Abs(angle - 270f) < tolerance)
        {
            return ThreeQuarters;
        }

        return Upright;
    }
}
