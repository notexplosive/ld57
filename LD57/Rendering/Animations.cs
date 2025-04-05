using System;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;
public delegate void AnimationFactory(TweenableGlyph glyph, SequenceTween tween);

public static class Animations
{

    public static AnimationFactory MakeWalk(Direction inputDirection, float pixels)
    {
        return (glyph, tween) =>
        {
            var horizontalSign = inputDirection.ToPoint().X;

            if (horizontalSign == 0)
            {
                horizontalSign = inputDirection.ToPoint().Y;
            }

            glyph.RootTween
                .Add(new MultiplexTween()
                    .Add(new SequenceTween()
                        .Add(glyph.Rotation.TweenTo(MathF.PI / 32 * horizontalSign, 0.1f, Ease.Linear))
                        .Add(glyph.Rotation.TweenTo(-MathF.PI / 32 * horizontalSign, 0.1f, Ease.Linear))
                        .Add(glyph.Rotation.TweenTo(0, 0.1f, Ease.Linear))
                    )
                    .Add(new SequenceTween()
                        .Add(glyph.PixelOffset.CallbackSetTo(inputDirection.ToGridCellSizedVector(-pixels)))
                        .Add(glyph.PixelOffset.TweenTo(inputDirection.ToGridCellSizedVector(pixels), 0.1f, Ease.Linear))
                        .Add(glyph.PixelOffset.TweenTo(Vector2.Zero, 0.1f, Ease.Linear))
                    )
                )
                ;
        };
    }

    public static AnimationFactory MakeMoveNudge(Direction inputDirection, float pixels)
    {
        return (glyph, tween) =>
        {
            tween
                .Add(glyph.PixelOffset.CallbackSetTo(inputDirection.ToGridCellSizedVector(-pixels)))
                .Add(glyph.PixelOffset.TweenTo(inputDirection.ToGridCellSizedVector(pixels), 0.1f, Ease.Linear))
                .Add(glyph.PixelOffset.TweenTo(Vector2.Zero, 0.1f, Ease.Linear))
                ;
        };
    }

    public static AnimationFactory MakeInPlaceNudge(Direction inputDirection, float pixels)
    {
        return (glyph, tween) =>
        {
            tween
                .Add(new SequenceTween()
                    .Add(glyph.PixelOffset.TweenTo(inputDirection.ToGridCellSizedVector(pixels), 0.1f, Ease.Linear))
                    .Add(glyph.PixelOffset.TweenTo(Vector2.Zero, 0.1f, Ease.Linear))
                )
                ;
        };
    }

    public static AnimationFactory PulseColorLoop(Color defaultColor, Color flashColor)
    {
        return (glyph, tween) =>
        {
            tween
                .Add(glyph.StartOverridingColor)
                .Add(glyph.ForegroundColorOverride.CallbackSetTo(defaultColor))
                .Add(glyph.ForegroundColorOverride.TweenTo(flashColor, 0.25f, Ease.Linear))
                .Add(new WaitSecondsTween(0.5f))
                .Add(glyph.ForegroundColorOverride.TweenTo(defaultColor, 0.25f, Ease.Linear))
                .Add(glyph.StopOverridingColor);
                ;

                tween.IsLooping = true;
        };
    }

    public static AnimationFactory FloatOnWater(Color backgroundColor)
    {
        return (glyph, tween) =>
        {
            tween
                .Add(new MultiplexTween()
                    .Add(new SequenceTween()
                        .Add(glyph.Scale.TweenTo(0.75f, 0.15f, Ease.Linear))
                    )
                    .Add(new SequenceTween()
                        .Add(glyph.BackgroundColor.TweenTo(Color.White, 0.02f, Ease.Linear))
                        .Add(glyph.BackgroundColor.TweenTo(backgroundColor, 0.15f, Ease.Linear))
                    )
                
                )
                .Add(new SequenceTween()
                    .Add(glyph.Scale.TweenTo(0.9f, 0.5f, Ease.Linear))
                    .Add(glyph.Scale.TweenTo(0.75f, 0.5f, Ease.Linear))
                    .SetLooping(true)
                )
                ;
        };
    }
}
