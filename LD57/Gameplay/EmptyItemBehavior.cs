using ExplogineMonoGame;
using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public class EmptyItemBehavior : ItemBehavior
{
    public override void Execute(World world)
    {
        Client.Debug.Log("Do nothing");
    }

    public override SequenceTween PlayAnimation(World world)
    {
        return new SequenceTween()
                .Add(ExecuteCallbackTween(world))
            ;
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity player, float dt)
    {
    }
    
    public override TweenableGlyph? GetTweenableGlyph()
    {
        return null;
    }
}
