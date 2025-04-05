using System.Collections.Generic;
using ExplogineMonoGame.Data;
using LD57.Rendering;
using LD57.Rules;

namespace LD57.Gameplay;

public class RuleComputer
{
    private readonly World _world;
    private List<IGameRule> _rules = new();

    public RuleComputer(World world)
    {
        _world = world;
    }

    public void AttemptMoveInDirection(Entity entity, Direction direction)
    {
        var oldPosition = entity.Position;
        var newPosition = entity.Position + new GridPosition(direction.ToPoint());
        entity.Position = newPosition;
        
        OnMoveCompleted(new MoveCompletedData(entity, oldPosition, newPosition));
    }

    public void AttemptWarp(Entity entity, GridPosition newPosition)
    {
        var oldPosition = entity.Position;
        entity.Position = newPosition;
        
        OnMoveCompleted(new MoveCompletedData(entity, oldPosition, newPosition));
    }

    private void OnMoveCompleted(MoveCompletedData moveCompletedData)
    {
        foreach (var rule in _rules)
        {
            rule.OnMoveCompleted(_world, moveCompletedData);
        }
    }

    public void AddRule(IGameRule rule)
    {
        _rules.Add(rule);
    }
}