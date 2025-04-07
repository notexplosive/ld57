using System;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class HookItemBehavior : ItemBehavior
{
    private readonly TweenableGlyph _iconTweenableGlyph = new();
    private Direction _cachedMoveDirection = Direction.None;

    public override void Execute(World world, Entity user)
    {
    }

    public override SequenceTween PlayAnimation(World world, Entity user)
    {
        return new SequenceTween()
                .Add(ExecuteCallbackTween(world, user))
            ;
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity user, float dt)
    {
        _iconTweenableGlyph.RootTween.Update(dt);

        var mostRecentDirection = user.MostRecentMoveDirection;
        if (mostRecentDirection != Direction.None && mostRecentDirection != _cachedMoveDirection)
        {
            _cachedMoveDirection = mostRecentDirection;
            _iconTweenableGlyph.RootTween.Clear();
            _iconTweenableGlyph.RootTween
                .Add(
                    _iconTweenableGlyph.Rotation.TweenTo(mostRecentDirection.Radians() + MathF.PI, 0.05f,
                        Ease.QuadFastSlow)
                )
                .Add(
                    new SequenceTween()
                        .Add(
                            _iconTweenableGlyph.PixelOffset.TweenTo(mostRecentDirection.ToGridCellSizedVector(20), 0.1f,
                                Ease.QuadFastSlow))
                        .Add(_iconTweenableGlyph.PixelOffset.TweenTo(Vector2.Zero, 0.1f,
                            Ease.QuadFastSlow))
                )
                ;
        }
    }

    public override TweenableGlyph? GetTweenableGlyph()
    {
        return _iconTweenableGlyph;
    }

    public override void OnRemove(World world, Entity player)
    {
        
    }
}
