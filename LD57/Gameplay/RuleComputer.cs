using System.Collections.Generic;
using ExplogineMonoGame.Data;
using LD57.Rendering;
using LD57.Rules;

namespace LD57.Gameplay;

public record struct MoveStatus(bool WasInterrupted);

public class RuleComputer
{
    private readonly World _world;
    private List<IGameRule> _rules = new();

    public RuleComputer(World world)
    {
        _world = world;
    }

    public MoveStatus AttemptMoveInDirection(Entity entity, Direction direction)
    {
        var status = new MoveStatus();
        var oldPosition = entity.Position;
        var newPosition = entity.Position + new GridPosition(direction.ToPoint());
        var moveData = new MoveData(entity, oldPosition, newPosition, direction);
        
        foreach (var rule in _rules)
        {
            if (rule.ShouldInterruptMove(_world, moveData))
            {
                status.WasInterrupted = true;
                break;
            }
        }

        if (!status.WasInterrupted)
        {
            entity.Position = newPosition;
        }

        OnMoveCompleted(moveData);
        return status;
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