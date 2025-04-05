using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public class TweenableGlyph
{
    public bool ShouldOverrideColor { get; private set; }

    public MultiplexTween RootTween { get; } = new();
    public TweenableVector2 PixelOffset { get; } = new();
    public TweenableFloat Rotation { get; } = new();
    public TweenableFloat Scale { get; } = new(1f);
    public TweenableColor ForegroundColorOverride { get; } = new(Color.White);
    public TweenableColor BackgroundColor { get; } = new(Color.Transparent);

    public CallbackTween StartOverridingColor => new(() => { ShouldOverrideColor = true; });
    public CallbackTween StopOverridingColor => new(() => { ShouldOverrideColor = false; });

    public void SkipCurrentAnimation()
    {
        RootTween.SkipToEnd();
        RootTween.Clear();
        ShouldOverrideColor = false;
        Rotation.Value = 0f;
        Scale.Value = 1f;
        PixelOffset.Value = Vector2.Zero;
        BackgroundColor.Value = Color.Transparent;
    }

    public void SetAnimation(AnimationFactory factory)
    {
        SkipCurrentAnimation();
        AddAnimation(factory);
    }

    public void AddAnimation(AnimationFactory factory)
    {
        var sequence = new SequenceTween();
        RootTween.Add(sequence);
        factory(this, sequence);
    }
}
