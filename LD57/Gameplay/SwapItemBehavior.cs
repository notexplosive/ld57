using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using LD57.Rendering;

namespace LD57.Gameplay;

public class SwapItemBehavior : ItemBehavior
{
    private readonly TweenableVector2 projectilePosition = new();
    private bool _showProjectile;
    
    public override void Execute(World world, Entity user)
    {
        var targetEntity = CalculateTargetEntity(world, user, out _);

        if (targetEntity == null)
        {
            return;
        }

        if (CanSwapWith(targetEntity))
        {
            // neat swap syntax!
            (user.Position, targetEntity.Position) = (targetEntity.Position, user.Position);
        }
    }

    private static bool CanSwapWith(Entity targetEntity)
    {
        return targetEntity.HasTag("Swappable");
    }

    private Entity? CalculateTargetEntity(World world, Entity user, out int distance)
    {
        if (user.MostRecentMoveDirection != Direction.None)
        {
            var range = 25;
            var scanOffset = new GridPosition(user.MostRecentMoveDirection);
            var scanPosition = user.Position;

            for (var i = 0; i < range; i++)
            {
                scanPosition += scanOffset;
                var hookStopper = FindStopper(world, scanPosition);
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

    private Entity? FindStopper(World world, GridPosition scanPosition)
    {
        foreach (var entity in world.GetActiveEntitiesAt(scanPosition))
        {
            // solids block the hook by default unless they opt out
            var isSolid = entity.HasTag("Solid") && !entity.HasTag("AllowSwapPassThrough");
            
            if (isSolid || entity.HasTag("Swappable"))
            {
                return entity;
            }
        }

        return null;
    }

    public override SequenceTween PlayAnimation(World world, Entity user)
    {
        var targetEntity = CalculateTargetEntity(world, user, out var distance);
        var fireDuration = 0.15f * distance / 10f + 0.1f;
        var falloffDuration = 0.05f;
        
        var smallScale = 0.5f;
        var normalScale = 1f;
        var bigScale = 1.5f;
        var swapColor = ResourceAlias.Color("swap");
        
        var tween = new SequenceTween()
                .Add(new CallbackTween(()=>_showProjectile = true))
                .Add(projectilePosition.CallbackSetTo(user.Position.ToPoint().ToVector2()))
                .Add(projectilePosition.TweenTo(
                    (user.Position + new GridPosition(user.MostRecentMoveDirection) * distance).ToPoint().ToVector2(),
                    fireDuration,
                    Ease.Linear))
                .Add(new CallbackTween(()=>_showProjectile = false))
            ;

        if (targetEntity != null && CanSwapWith(targetEntity))
        {
            tween
                .Add(new MultiplexTween()
                    .Add(user.TweenableGlyph.Scale.TweenTo(smallScale, falloffDuration, Ease.Linear))
                    .Add(targetEntity.TweenableGlyph.Scale.TweenTo(smallScale, falloffDuration, Ease.Linear))
                    .Add(user.TweenableGlyph.StartOverridingColor)
                    .Add(user.TweenableGlyph.ForegroundColorOverride.TweenTo(swapColor, falloffDuration,
                        Ease.Linear))
                    .Add(targetEntity.TweenableGlyph.StartOverridingColor)
                    .Add(targetEntity.TweenableGlyph.ForegroundColorOverride.TweenTo(swapColor, falloffDuration,
                        Ease.Linear))
                )
                .Add(ExecuteCallbackTween(world, user))
                .Add(
                    new MultiplexTween()
                        .Add(user.TweenableGlyph.Scale.TweenTo(bigScale, falloffDuration, Ease.Linear))
                        .Add(targetEntity.TweenableGlyph.Scale.TweenTo(bigScale, falloffDuration, Ease.Linear))
                )
                .Add(
                    new MultiplexTween()
                        .Add(user.TweenableGlyph.Scale.TweenTo(normalScale, falloffDuration, Ease.Linear))
                        .Add(targetEntity.TweenableGlyph.Scale.TweenTo(normalScale, falloffDuration, Ease.Linear))
                )
                .Add(user.TweenableGlyph.StopOverridingColor)
                .Add(targetEntity.TweenableGlyph.StopOverridingColor)
                ;
        }
        else
        {
            tween
                .Add(ExecuteCallbackTween(world, user));
        }
        
        return tween;
    }

    public override void PaintInWorld(AsciiScreen screen, World world, Entity user, float dt)
    {
        if (_showProjectile)
        {
            var x = new GridPosition(projectilePosition.Value.ToPoint()) - world.CameraPosition;
            screen.PutTile(x, TileState.Sprite(ResourceAlias.Entities, 8, ResourceAlias.Color("swap")));
        }
    }

    public override TweenableGlyph? GetTweenableGlyph()
    {
        return null;
    }

    public override void OnRemove(World world, Entity player)
    {
        
    }
}
