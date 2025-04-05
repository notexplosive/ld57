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
        var data = new MoveData(
            mover,
            mover.Position,
            mover.Position + new GridPosition(direction.ToPoint()),
            direction);
        var status = new MoveStatus(data);

        var entitiesAtDestination = _world.GetActiveEntitiesAt(data.Destination).ToList();

        if (mover.HasTag("Solid"))
        {
            var solidEntitiesAtDestination = _world.FilterToEntitiesWithTag(entitiesAtDestination, "Solid").ToList();
            var waterEntitiesAtDestination = _world.FilterToEntitiesWithTag(entitiesAtDestination, "Water").ToList();
            
            if (solidEntitiesAtDestination.Count > 0)
            {
                if (!mover.HasTag("Pusher"))
                {
                    status.Interrupt();
                }
                else
                {
                    foreach (var solidEntity in solidEntitiesAtDestination)
                    {
                        if (solidEntity.HasTag("Pushable"))
                        {
                            var secondaryMove = _world.Rules.AttemptMoveInDirection(solidEntity, data.Direction);
                            status.DependOnMove(secondaryMove);
                        }
                        else
                        {
                            status.Interrupt();
                        }
                    }
                }
            }

            if (waterEntitiesAtDestination.Count > 0)
            {
                if (!mover.HasTag("FloatsInWater"))
                {
                    status.Interrupt();
                }

                if (mover.HasTag("FillsWater"))
                {
                    foreach (var water in waterEntitiesAtDestination)
                    {
                        _world.Remove(water);
                    }

                    mover.SetActive(false);
                }
            }
        }

        if (!status.WasInterrupted)
        {
            mover.Position = mover.Position + new GridPosition(direction.ToPoint());
        }

        OnMoveCompleted(data, status);
        return status;
    }

    public void AttemptWarp(Entity entity, GridPosition newPosition)
    {
        var status = new MoveStatus();
        var oldPosition = entity.Position;
        entity.Position = newPosition;

        OnMoveCompleted(new MoveData(entity, oldPosition, newPosition, Direction.None), status);
    }

    private void OnMoveCompleted(MoveData moveData, MoveStatus status)
    {
        _world.OnMoveCompleted(moveData, status);
        
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
