using ExplogineMonoGame;
using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public class EmptyItemBehavior : ItemBehavior
{
    public EmptyItemBehavior()
    {
        DefaultHudTile = TileState.Empty;
    }
    
    public override void Execute(World world, Entity user)
    {
        Client.Debug.Log("Do nothing");
    }

    public override SequenceTween PlayAnimation(World world, Entity user)
    {
        return new SequenceTween()
                .Add(ExecuteCallbackTween(world, user))
            ;
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity user, float dt)
    {
    }
    
    public override TweenableGlyph? GetTweenableGlyph()
    {
        return null;
    }

    public override void OnRemove(World world, Entity player)
    {
        
    }
}
