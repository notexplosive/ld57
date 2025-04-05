using System;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public static class Animations
{
    public static Action<TweenableGlyph> MakeWalk(Direction inputDirection, float pixels)
    {
        return glyph =>
        {
            var horizontalSign = inputDirection.ToPoint().X;

            if (horizontalSign == 0)
            {
                horizontalSign = inputDirection.ToPoint().Y;
            }

            glyph.Tween
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

    public static Action<TweenableGlyph> MakeMoveNudge(Direction inputDirection, float pixels)
    {
        return glyph =>
        {
            glyph.Tween
                .Add(glyph.PixelOffset.CallbackSetTo(inputDirection.ToGridCellSizedVector(-pixels)))
                .Add(glyph.PixelOffset.TweenTo(inputDirection.ToGridCellSizedVector(pixels), 0.1f, Ease.Linear))
                .Add(glyph.PixelOffset.TweenTo(Vector2.Zero, 0.1f, Ease.Linear))
                ;
        };
    }

    public static Action<TweenableGlyph> MakeInPlaceNudge(Direction inputDirection, float pixels)
    {
        return glyph =>
        {
            glyph.Tween
                .Add(new SequenceTween()
                    .Add(glyph.PixelOffset.TweenTo(inputDirection.ToGridCellSizedVector(pixels), 0.1f, Ease.Linear))
                    .Add(glyph.PixelOffset.TweenTo(Vector2.Zero, 0.1f, Ease.Linear))
                )
                ;
        };
    }
}

public class TweenableGlyph
{
    private readonly TweenableColor _tweenableColor = new(Color.White);
    public bool ShouldOverrideColor { get; private set; }

    public SequenceTween Tween { get; } = new();
    public TweenableVector2 PixelOffset { get; } = new();
    public TweenableFloat Rotation { get; } = new();
    public TweenableFloat Scale { get; } = new(1f);

    public TweenableColor ColorOverride
    {
        get
        {
            ShouldOverrideColor = true;
            return _tweenableColor;
        }
    }

    public CallbackTween StopOverridingColor => new(() => { ShouldOverrideColor = false; });

    public void Animate(Action<TweenableGlyph> applyAnimation)
    {
        Tween.SkipToEnd();
        Tween.Clear();
        applyAnimation(this);
    }
}
