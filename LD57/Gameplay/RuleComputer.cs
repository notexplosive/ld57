using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame.Data;
using LD57.Rendering;
using LD57.Rules;

namespace LD57.Gameplay;

public class RuleComputer
{
    private readonly List<IGameRule> _rules = new();
    private readonly World _world;

    public RuleComputer(World world)
    {
        _world = world;
    }

    public MoveStatus AttemptMoveInDirection(Entity mover, Direction direction)
    {
        var move = new MoveStatus();
        var oldPosition = mover.Position;
        var newPosition = mover.Position + new GridPosition(direction.ToPoint());
        var moveData = new MoveData(mover, oldPosition, newPosition, direction);

        var entitiesAtDestination = _world.GetEntitiesAt(moveData.NewPosition).ToList();

        if (mover.HasTag("Solid"))
        {
            var solidEntitiesAtDestination = EntitiesWithTag(entitiesAtDestination, "Solid").ToList();
            if (solidEntitiesAtDestination.Count > 0)
            {
                if (!mover.HasTag("Pusher"))
                {
                    move.Interrupt();
                }
                else
                {
                    foreach (var solidEntity in solidEntitiesAtDestination)
                    {
                        if (solidEntity.HasTag("Pushable"))
                        {
                            var secondaryMove = _world.Rules.AttemptMoveInDirection(solidEntity, moveData.Direction);
                            move.DependOnMove(secondaryMove);
                        }
                        else
                        {
                            move.Interrupt();
                        }
                    }
                }
            }
        }

        if (!move.WasInterrupted)
        {
            mover.Position = newPosition;
        }

        OnMoveCompleted(moveData);
        return move;
    }

    private IEnumerable<Entity> EntitiesWithTag(List<Entity> entities, string tag)
    {
        foreach (var entity in entities)
        {
            if (entity.HasTag(tag))
            {
                yield return entity;
            }
        }
    }

    public void AttemptWarp(Entity entity, GridPosition newPosition)
    {
        var oldPosition = entity.Position;
        entity.Position = newPosition;

        OnMoveCompleted(new MoveData(entity, oldPosition, newPosition, Direction.None));
    }

    private void OnMoveCompleted(MoveData moveData)
    {
        foreach (var rule in _rules)
        {
            rule.OnMoveCompleted(_world, moveData);
        }
    }

    public void AddRule(IGameRule rule)
    {
        _rules.Add(rule);
    }
}
