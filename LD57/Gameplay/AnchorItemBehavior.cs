using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public class AnchorItemBehavior : ItemBehavior
{
    public override void Execute(World world)
    {
        
    }

    public override SequenceTween PlayAnimation(World world)
    {
        return new();
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity player, float dt)
    {
        
    }
    
    public override TweenableGlyph? GetTweenableGlyph()
    {
        return null;
    }
}
