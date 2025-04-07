using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public abstract class ItemBehavior
{
    public TileState DefaultHudTile { get; private set; } = TileState.StringCharacter("?");

    public abstract void Execute(World world);

    public CallbackTween ExecuteCallbackTween(World world)
    {
        return new CallbackTween(() => { Execute(world); });
    }

    /// <summary>
    ///     This is assumed to call ExecuteCallbackTween() at some point
    /// </summary>
    public abstract SequenceTween PlayAnimation(World world);

    public abstract void PaintInWorld(AsciiScreen screen, World world, Entity player, float dt);

    public void SetUiTile(TileState tileState)
    {
        DefaultHudTile = tileState;
    }

    public abstract TweenableGlyph? GetTweenableGlyph();
}
