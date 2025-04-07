using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public abstract class ItemBehavior
{
    public TileState DefaultHudTile { get; protected set; } = TileState.StringCharacter("?");

    public abstract void Execute(World world, Entity user);

    public CallbackTween ExecuteCallbackTween(World world, Entity user)
    {
        return new CallbackTween(() => { Execute(world, user); });
    }

    /// <summary>
    ///     This is assumed to call ExecuteCallbackTween() at some point
    /// </summary>
    public abstract SequenceTween PlayAnimation(World world, Entity user);

    public abstract void PaintInWorld(AsciiScreen screen, World world, Entity user, float dt);

    public void SetUiTile(TileState tileState)
    {
        DefaultHudTile = tileState;
    }

    public abstract TweenableGlyph? GetTweenableGlyph();

    public abstract void OnRemove(World world, Entity player);
}
