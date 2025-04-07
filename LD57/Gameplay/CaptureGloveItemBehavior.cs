using System;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class CaptureGloveItemBehavior : ItemBehavior
{
    private readonly TweenableGlyph _iconTweenableGlyph = new();
    private readonly TweenableGlyph _rejectionTweenable;
    private Direction _cachedMoveDirection = Direction.None;
    private Entity? _capturedEntity;
    private TileState _savedDefaultHudTile;
    private bool _showRejection;

    public CaptureGloveItemBehavior()
    {
        _rejectionTweenable = new TweenableGlyph();
    }

    public override void Execute(World world, Entity user)
    {
        if (user.MostRecentMoveDirection != Direction.None)
        {
            var targetPosition = TargetPosition(user);

            if (_capturedEntity == null)
            {
                var thingToCapture = CalculateThingToCapture(world, targetPosition);
                if (thingToCapture != null)
                {
                    world.Destroy(thingToCapture);
                    _capturedEntity = thingToCapture;
                }
            }
            else
            {
                if (CanDropHere(world, targetPosition))
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

    private static GridPosition TargetPosition(Entity user)
    {
        return user.Position + new GridPosition(user.MostRecentMoveDirection);
    }

    private bool CanDropHere(World world, GridPosition targetPosition)
    {
        if (_capturedEntity == null)
        {
            return false;
        }

        var isEmptyAtTarget = world.GetActiveEntitiesAt(targetPosition).All(a => a.Appearance?.TileState.HasValue != true);
        return isEmptyAtTarget;
    }

    private static Entity? CalculateThingToCapture(World world, GridPosition targetPosition)
    {
        var thingToCapture = world.GetActiveEntitiesAt(targetPosition).OrderBy(a => a.SortPriority)
            .FirstOrDefault(a => a.HasTag("Capturable"));
        return thingToCapture;
    }

    public override SequenceTween PlayAnimation(World world, Entity user)
    {
        var targetPosition = TargetPosition(user);
        if (_capturedEntity == null)
        {
            var thingToCapture = CalculateThingToCapture(world, targetPosition);

            if (thingToCapture == null)
            {
                return RejectAnimation(world, user);
            }

            return CaptureAnimation(world, user);
        }

        var canDrop = CanDropHere(world, targetPosition);

        if (canDrop)
        {
            return DropAnimation(world, user);
        }

        return RejectAnimation(world, user);
    }

    private SequenceTween CaptureAnimation(World world, Entity user)
    {
        var entityToCapture = CalculateThingToCapture(world, TargetPosition(user));

        if (entityToCapture == null)
        {
            return InstantExecute(world, user);
        }

        var duration = 0.15f;

        return new SequenceTween()
                .Add(ResourceAlias.CallbackPlaySound("brush", new SoundEffectSettings()))
                .Add(new CallbackTween(() => { entityToCapture.TweenableGlyph.SkipCurrentAnimation(); }))
                .Add(new MultiplexTween()
                    .Add(entityToCapture.TweenableGlyph.Scale.TweenTo(0.15f, duration, Ease.QuadSlowFast))
                    .Add(entityToCapture.TweenableGlyph.PixelOffset.TweenTo(
                        user.MostRecentMoveDirection.ToGridCellSizedVector(-60), duration / 2f, Ease.QuadSlowFast))
                )
                .Add(new CallbackTween(() =>
                {
                    _savedDefaultHudTile = DefaultHudTile;
                    DefaultHudTile = entityToCapture.TileState ?? _savedDefaultHudTile;
                }))
                .Add(ExecuteCallbackTween(world, user))
            ;
    }

    private SequenceTween DropAnimation(World world, Entity user)
    {
        var entityToDrop = _capturedEntity;

        if (entityToDrop == null)
        {
            // shouldn't happen, but just to be safe
            return InstantExecute(world, user);
        }

        var duration = 0.15f;

        return new SequenceTween()
                .Add(ResourceAlias.CallbackPlaySound("brush", new SoundEffectSettings{Pitch = -1f}))
                .Add(new CallbackTween(() => { entityToDrop.TweenableGlyph.SkipCurrentAnimation(); }))
                .Add(ExecuteCallbackTween(world, user))
                .Add(new CallbackTween(() => { DefaultHudTile = _savedDefaultHudTile; }))
                .Add(entityToDrop.TweenableGlyph.Scale.CallbackSetTo(0.5f))
                .Add(entityToDrop.TweenableGlyph.PixelOffset.CallbackSetTo(user.MostRecentMoveDirection
                    .ToGridCellSizedVector(-60)))
                .Add(new MultiplexTween()
                    .Add(new SequenceTween()
                        .Add(entityToDrop.TweenableGlyph.Scale.TweenTo(1.25f, duration/2f, Ease.QuadSlowFast))
                        .Add(entityToDrop.TweenableGlyph.Scale.TweenTo(1f, duration/2f, Ease.QuadFastSlow))
                    )
                    .Add(entityToDrop.TweenableGlyph.PixelOffset.TweenTo(Vector2.Zero, duration / 2f,
                        Ease.QuadFastSlow))
                )
            ;
    }

    private SequenceTween InstantExecute(World world, Entity user)
    {
        return new SequenceTween()
            .Add(ExecuteCallbackTween(world, user));
    }

    private SequenceTween RejectAnimation(World world, Entity user)
    {
        var error = ResourceAlias.Color("blood");
        var normal = ResourceAlias.Color("white");
        return new SequenceTween()
            .Add(ResourceAlias.CallbackPlaySound("clank", new SoundEffectSettings{Pitch =1f}))
            .Add(new CallbackTween(() => _showRejection = true))
            .Add(new MultiplexTween()
                .Add(new SequenceTween()
                    .Add(_rejectionTweenable.StartOverridingColor)
                    .Add(_rejectionTweenable.ForegroundColorOverride.TweenTo(error, 0.1f, Ease.Linear))
                    .Add(_rejectionTweenable.ForegroundColorOverride.TweenTo(normal, 0.1f, Ease.Linear))
                    .Add(_rejectionTweenable.StopOverridingColor)
                )
                .Add(
                    new SequenceTween()
                        .Add(_rejectionTweenable.Scale.TweenTo(1.25f, 0.1f, Ease.Linear))
                        .Add(_rejectionTweenable.Scale.TweenTo(1, 0.05f, Ease.Linear))
                )
            )
            .Add(new CallbackTween(() => _showRejection = false))
            .Add(ExecuteCallbackTween(world, user));
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity user, float dt)
    {
        UpdateIcon(user, dt);

        if (_showRejection)
        {
            var rejectionTile = TileState.StringCharacter("X", Color.White);
            if (_capturedEntity != null)
            {
                rejectionTile = _capturedEntity.TileState ?? rejectionTile;
            }

            screen.PutTile(TargetPosition(user) - world.CameraPosition, rejectionTile, _rejectionTweenable);
        }
    }

    private void UpdateIcon(Entity player, float dt)
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
