using System;
using System.Linq;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class CaptureGloveItemBehavior : ItemBehavior
{
    private readonly TweenableGlyph _iconTweenableGlyph = new();
    private Direction _cachedMoveDirection = Direction.None;
    private Entity? _capturedEntity;

    public override void Execute(World world, Entity user)
    {
        if (user.MostRecentMoveDirection != Direction.None)
        {
            var targetPosition = user.Position + new GridPosition(user.MostRecentMoveDirection.ToPoint());

            if (_capturedEntity == null)
            {
                var thingToCapture = world.GetActiveEntitiesAt(targetPosition).OrderBy(a => a.SortPriority)
                    .FirstOrDefault(a => a.HasTag("Capturable"));
                if (thingToCapture != null)
                {
                    world.Destroy(thingToCapture);
                    _capturedEntity = thingToCapture;
                }
            }
            else
            {
                if (world.Rules.CouldMoveTo(_capturedEntity, targetPosition))
                {
                    if (_capturedEntity.HasTag("ClearOnDrop"))
                    {
                        foreach (var entities in world.GetActiveEntitiesAt(targetPosition))
                        {
                            world.Destroy(entities);
                        }
                    }

                    _capturedEntity.Position = targetPosition;
                    world.AddEntity(_capturedEntity);
                    _capturedEntity = null;
                }
            }
        }
    }

    public override SequenceTween PlayAnimation(World world, Entity user)
    {
        return new SequenceTween()
                .Add(ExecuteCallbackTween(world, user))
            ;
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity player, float dt)
    {
        _iconTweenableGlyph.RootTween.Update(dt);

        var mostRecentDirection = player.MostRecentMoveDirection;
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
