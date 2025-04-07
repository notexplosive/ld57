﻿using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public class AnchorItemBehavior : ItemBehavior
{
    private Entity? _anchorEntity;

    public override void Execute(World world, Entity user)
    {
        if (_anchorEntity == null || world.IsDestroyed(_anchorEntity))
        {
            _anchorEntity = world.AddEntity(new Entity(user.Position, PhantomTemplate()));
        }
        else
        {
            var previousPosition = user.Position;
            world.Rules.WarpToPosition(user, _anchorEntity.Position);
            _anchorEntity.Position = previousPosition;
        }
    }

    private static EntityTemplate PhantomTemplate()
    {
        return ResourceAlias.EntityTemplate("phantom_anchor");
    }

    public override SequenceTween PlayAnimation(World world, Entity user)
    {
        var beforeWarp = new SequenceTween();
        var afterWarp = new SequenceTween();

        var snappyDuration = 0.05f;
        var smallScale = 0.5f;
        var normalScale = 1f;
        var bigScale = 1.5f;
        var anchorColor = ResourceAlias.Color(PhantomTemplate().Color);

        if (_anchorEntity != null)
        {
            // fade both at once, snappy!
            beforeWarp
                .Add(
                    new MultiplexTween()
                        .Add(user.TweenableGlyph.Scale.TweenTo(smallScale, snappyDuration, Ease.Linear))
                        .Add(_anchorEntity.TweenableGlyph.Scale.TweenTo(smallScale, snappyDuration, Ease.Linear))
                        .Add(user.TweenableGlyph.StartOverridingColor)
                        .Add(user.TweenableGlyph.ForegroundColorOverride.TweenTo(anchorColor, snappyDuration,
                            Ease.Linear))
                );

            afterWarp
                .Add(
                    new MultiplexTween()
                        .Add(user.TweenableGlyph.Scale.TweenTo(bigScale, snappyDuration, Ease.Linear))
                        .Add(_anchorEntity.TweenableGlyph.Scale.TweenTo(bigScale, snappyDuration, Ease.Linear))
                )
                .Add(
                    new MultiplexTween()
                        .Add(user.TweenableGlyph.Scale.TweenTo(normalScale, snappyDuration, Ease.Linear))
                        .Add(_anchorEntity.TweenableGlyph.Scale.TweenTo(normalScale, snappyDuration, Ease.Linear))
                )
                .Add(user.TweenableGlyph.StopOverridingColor)
                ;
        }
        else
        {
            // Drop anchor for the first time
            afterWarp
                .Add(new MultiplexTween()
                    .Add(new SequenceTween()
                        .Add(user.TweenableGlyph.StartOverridingColor)
                        .Add(user.TweenableGlyph.ForegroundColorOverride.TweenTo(anchorColor, snappyDuration * 2,
                            Ease.Linear))
                        .Add(user.TweenableGlyph.StopOverridingColor)
                    )
                    .Add(new SequenceTween()
                        .Add(user.TweenableGlyph.Scale.TweenTo(bigScale, snappyDuration, Ease.Linear))
                        .Add(user.TweenableGlyph.Scale.TweenTo(normalScale, snappyDuration, Ease.Linear))
                    )
                )
                ;
        }

        return new SequenceTween()
                .Add(beforeWarp)
                .Add(ExecuteCallbackTween(world, user))
                .Add(afterWarp)
            ;
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity player, float dt)
    {
    }

    public override TweenableGlyph? GetTweenableGlyph()
    {
        return null;
    }

    public override void OnRemove(World world, Entity player)
    {
        if (_anchorEntity != null)
        {
            world.Destroy(_anchorEntity);
        }
    }
}
