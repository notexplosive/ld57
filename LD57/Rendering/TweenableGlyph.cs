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
    public TweenableColor ColorOverride { get; } = new(Color.White);

    public CallbackTween StartOverridingColor => new(() => { ShouldOverrideColor = true; });
    public CallbackTween StopOverridingColor => new(() => { ShouldOverrideColor = false; });

    public void SkipCurrentAnimation()
    {
        RootTween.SkipToEnd();
        RootTween.Clear();
        ShouldOverrideColor = false;
    }

    public void SetAnimation(Animations.AnimationFactory factory)
    {
        SkipCurrentAnimation();
        AddAnimation(factory);
    }

    public void AddAnimation(Animations.AnimationFactory factory)
    {
        var sequence = new SequenceTween();
        RootTween.Add(sequence);
        factory(this, sequence);
    }
}
