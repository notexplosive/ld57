using System;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class HookItemBehavior : ItemBehavior
{
    private readonly TweenableVector2 _chainHeadPosition = new();
    private readonly TweenableVector2 _dummyPosition = new();
    private readonly TweenableGlyph _iconTweenableGlyph = new();
    private Direction _cachedMoveDirection = Direction.None;
    private bool _showChain;
    private bool _showDummyUser;
    private Entity? _currentPulledThing;

    public override void Execute(World world, Entity user)
    {
        var targetEntity = CalculateTargetEntity(world, user, out _);

        if (targetEntity == null)
        {
            return;
        }

        if (ShouldPullTarget(targetEntity))
        {
            var destination = user.Position + new GridPosition(user.MostRecentMoveDirection);
            world.Rules.WarpToPosition(targetEntity, destination);
        }
        else if (TargetShouldPullMe(targetEntity))
        {
            var destination = CalculateMyLandingDestination(world, user, targetEntity.Position);
            world.Rules.WarpToPosition(user, destination);
        }
    }

    private GridPosition CalculateMyLandingDestination(World world, Entity user, GridPosition position)
    {
        if (world.Rules.CouldMoveTo(user, position))
        {
            return position;
        }

        return position - new GridPosition(user.MostRecentMoveDirection.ToPoint());
    }

    private bool TargetShouldPullMe(Entity targetEntity)
    {
        return targetEntity.HasTag("HookAnchor");
    }

    private static bool ShouldPullTarget(Entity targetEntity)
    {
        return targetEntity.HasTag("PulledByHook");
    }

    private Entity? CalculateTargetEntity(World world, Entity user, out int distance)
    {
        if (user.MostRecentMoveDirection != Direction.None)
        {
            var range = 10;
            var scanOffset = new GridPosition(user.MostRecentMoveDirection);
            var scanPosition = user.Position;

            for (var i = 0; i < range; i++)
            {
                scanPosition += scanOffset;
                var hookStopper = FindHookStopper(world, scanPosition);
                if (hookStopper != null)
                {
                    distance = i;
                    return hookStopper;
                }
            }

            distance = range;
            return null;
        }

        distance = 0;
        return null;
    }

    private Entity? FindHookStopper(World world, GridPosition scanPosition)
    {
        foreach (var entity in world.GetActiveEntitiesAt(scanPosition))
        {
            // solids block the hook by default unless they opt out
            var isSolid = entity.HasTag("Solid") && !entity.HasTag("AllowsHookPassThrough");

            // stop for any entity that interacts with the hook in some way
            if (isSolid || entity.HasTag("BlocksHook") ||
                entity.HasTag("PulledByHook") || entity.HasTag("HookAnchor"))
            {
                return entity;
            }
        }

        return null;
    }

    public override SequenceTween PlayAnimation(World world, Entity user)
    {
        var targetEntity = CalculateTargetEntity(world, user, out var distance);
        var extendDuration = 0.35f * distance / 10f + 0.1f;

        if (targetEntity == null)
        {
            return new SequenceTween()
                    .Add(new CallbackTween(() => _showChain = true))
                    .Add(_chainHeadPosition.CallbackSetTo(user.Position.ToPoint().ToVector2()))
                    .Add(_chainHeadPosition.TweenTo((user.Position + new GridPosition(user.MostRecentMoveDirection) * distance).ToPoint().ToVector2(), extendDuration,
                        Ease.QuadFastSlow))
                    .Add(ExecuteCallbackTween(world, user))
                    .Add(_chainHeadPosition.TweenTo(user.Position.ToPoint().ToVector2(), extendDuration, Ease.QuadFastSlow))
                    .Add(new CallbackTween(() => _showChain = false))
                ;
        }


        var tween = new SequenceTween()
                .Add(new CallbackTween(() => _showChain = true))
                .Add(_chainHeadPosition.CallbackSetTo(user.Position.ToPoint().ToVector2()))
                .Add(_chainHeadPosition.TweenTo(targetEntity.Position.ToPoint().ToVector2(), extendDuration,
                    Ease.QuadFastSlow))
            ;

        if (ShouldPullTarget(targetEntity))
        {
            tween
                .Add(new WaitSecondsTween(0.1f))
                .Add(new CallbackTween(()=>_currentPulledThing = targetEntity))
                .Add(new WaitSecondsTween(0.2f))
                .Add(ExecuteCallbackTween(world, user))
                .Add(_chainHeadPosition.TweenTo(user.Position.ToPoint().ToVector2(), extendDuration, Ease.QuadFastSlow))
                .Add(new CallbackTween(()=>_currentPulledThing = null))
                ;
        }
        else if (TargetShouldPullMe(targetEntity))
        {
            var destination = CalculateMyLandingDestination(world, user, targetEntity.Position);

            tween
                .Add(new WaitSecondsTween(0.1f))
                .Add(ExecuteCallbackTween(world, user))
                .Add(new CallbackTween(() => { _showDummyUser = true; }))
                .Add(_dummyPosition.CallbackSetTo(user.Position.ToPoint().ToVector2()))
                .Add(_dummyPosition.TweenTo(destination.ToPoint().ToVector2(), extendDuration, Ease.QuadFastSlow))
                .Add(new CallbackTween(() => _showDummyUser = false))
                ;
        }
        else
        {
            // reject target
            tween
                .Add(ExecuteCallbackTween(world, user))
                .Add(_chainHeadPosition.TweenTo(user.Position.ToPoint().ToVector2(), extendDuration, Ease.QuadFastSlow))
                ;
        }

        tween
            .Add(new CallbackTween(() => _showChain = false))
            ;

        return tween;
    }

    private SequenceTween InstantExecute(World world, Entity user)
    {
        return new SequenceTween()
                .Add(ExecuteCallbackTween(world, user))
            ;
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity user, float dt)
    {
        if (_showChain)
        {
            var chainHeadPosition =
                new GridPosition(_chainHeadPosition.Value.Rounded().ToPoint()) - world.CameraPosition;
            
            
            var userPosition = user.Position - world.CameraPosition;

            if (_showDummyUser)
            {
                userPosition = 
                    new GridPosition(_dummyPosition.Value.Rounded().ToPoint()) - world.CameraPosition;
            }
            
            if (chainHeadPosition != userPosition)
            {
                var hookBodyTile = TileState.Sprite(ResourceAlias.Entities, 7, ResourceAlias.Color("hook"));
                screen.PutFilledRectangle(hookBodyTile,
                    chainHeadPosition, userPosition + new GridPosition(user.MostRecentMoveDirection));
                

                var hookHeadTile = TileState.Sprite(ResourceAlias.Entities, 5, ResourceAlias.Color("hook"));
                if (_currentPulledThing != null)
                {
                    var tile = _currentPulledThing.TileState ?? hookBodyTile;
                    screen.PutTile(chainHeadPosition, tile, _currentPulledThing.TweenableGlyph);
                }
                else
                {
                    screen.PutTile(chainHeadPosition, hookHeadTile, _iconTweenableGlyph);
                }
            }

            if (_showDummyUser)
            {
                screen.PutTile(userPosition, user.TileState!.Value, user.TweenableGlyph);
            }
        }

        UpdateIcon(user, dt);
    }

    private void UpdateIcon(Entity user, float dt)
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
