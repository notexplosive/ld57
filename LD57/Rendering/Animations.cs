using System;
using ExplogineMonoGame;
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
                horizontalSign = Client.Random.Dirty.NextSign();
            }

            glyph.RootTween
                .Add(new MultiplexTween()
                    // .Add(new SequenceTween()
                    //     .Add(glyph.Rotation.TweenTo(MathF.PI / 32 * horizontalSign, 0.1f, Ease.Linear))
                    //     .Add(glyph.Rotation.TweenTo(-MathF.PI / 32 * horizontalSign, 0.1f, Ease.Linear))
                    //     .Add(glyph.Rotation.TweenTo(0, 0.1f, Ease.Linear))
                    // )
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
                    .Add(glyph.PixelOffset.TweenTo(inputDirection.ToGridCellSizedVector(pixels), 0.05f, Ease.Linear))
                    .Add(glyph.PixelOffset.TweenTo(Vector2.Zero, 0.05f, Ease.Linear))
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

    public static AnimationFactory FloatOnWater()
    {
        return (glyph, tween) =>
        {
            tween
                .Add(new MultiplexTween()
                    .Add(new SequenceTween()
                        .Add(glyph.Scale.TweenTo(0.75f, 0.15f, Ease.Linear))
                    )
                )
                .Add(
                    new MultiplexTween()
                        .Add(new SequenceTween()
                            .Add(glyph.Scale.TweenTo(0.9f, 1f, Ease.Linear))
                            .Add(glyph.Scale.TweenTo(0.8f, 1f, Ease.Linear))
                            .SetLooping(true)
                        )                  
                    )
                ;
        };
    }

    public static AnimationFactory WaterSway(float randomFloat)
    {
        return (glyph, tween) =>
        {
            var swaySegmentDuration = 0.73f;
            var growDuration = 1.44f;
            var bigScale = 1.15f;
            var midScale = 1.1f;
            var lowScale = 1f;

            var swayAmount = 2;
            
            tween
                .Add(new SequenceTween()
                    .Add(glyph.PixelOffset.TweenTo(new Vector2(-swayAmount, 0), swaySegmentDuration, Ease.SineFastSlow))
                    .Add(glyph.PixelOffset.TweenTo(new Vector2(0, 0), swaySegmentDuration, Ease.SineSlowFast))
                    .Add(glyph.PixelOffset.TweenTo(new Vector2(swayAmount, 0), swaySegmentDuration, Ease.SineFastSlow))
                    .Add(glyph.PixelOffset.TweenTo(new Vector2(0, 0), swaySegmentDuration, Ease.SineSlowFast))
                    .SetLooping(true)
                )
                .Add(new SequenceTween()
                    .Add(glyph.Scale.TweenTo(bigScale, growDuration, Ease.SineFastSlow))
                    .Add(glyph.Scale.TweenTo(midScale, growDuration, Ease.SineSlowFast))
                    .Add(glyph.Scale.TweenTo(lowScale, growDuration, Ease.SineFastSlow))
                    .Add(glyph.Scale.TweenTo(midScale, growDuration, Ease.SineSlowFast))
                )
                ;

            tween.Update(randomFloat);
        };
    }
}
