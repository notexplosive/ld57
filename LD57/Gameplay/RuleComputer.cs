using System;
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
        var status = EvaluateMove(data);
        status.ExecuteActions();

        if (status.WasSuccessful)
        {
            mover.Position = data.Destination;
        }

        OnMoveCompleted(data, status);
        return status;
    }

    public void WarpToPosition(Entity mover, GridPosition newPosition)
    {
        var data = new MoveData(
            mover,
            mover.Position,
            newPosition,
            mover.MostRecentMoveDirection);
        var status = EvaluateMove(data);
        status.ExecuteActions();

        if (status.WasSuccessful)
        {
            // warp doesn't care if it succeeds or not
        }

        mover.Position = newPosition;

        OnMoveCompleted(data, status);
    }

    private MoveStatus EvaluateMove(MoveData data)
    {
        var status = new MoveStatus();
        var mover = data.Mover;

        var targets = _world.GetActiveEntitiesAt(data.Destination).ToList();
        targets.Remove(data.Mover);

        foreach (var target in targets)
        {
            var tags = new TagComparer(mover, target);
            if (tags.Check(
                    [Is("Solid")],
                    [Is("Solid")]))
            {
                if (tags.Check(
                        [Is("Pusher")],
                        [Is("Pushable")]))
                {
                    status.DependOnMove(_world.Rules.AttemptMoveInDirection(target, data.Direction));
                }
                else
                {
                    status.Fail();
                }
            }

            if (tags.Check(
                    [IsNotAnyOf("FloatsInWater", "FillsWater")],
                    [Is("Water")]))
            {
                status.Fail();
            }

            if (tags.Check(
                    [Is("Solid")],
                    [Is("Door"), StateBool("is_open", false)]
                ))
            {
                status.Fail();
            }

            if (tags.Check(
                    [Is("FillsWater"), Is("FloatsInWater")],
                    [Is("Water")]))
            {
                status.AddAction(() =>
                {
                    _world.Destroy(target);
                    mover.SetActive(false);
                });
            }

            if (tags.Check(
                    [Is("FillsWater"), IsNot("FloatsInWater")],
                    [Is("Water")]))
            {
                status.AddAction(() =>
                {
                    _world.Destroy(target);
                    _world.Destroy(mover);
                });
            }
        }

        return status;
    }

    private ICondition StateBool(string key, bool value)
    {
        return new StateValueBoolean(key, value, false);
    }

    private ICondition IsNotAnyOf(params string[] tags)
    {
        return new ManyTagOwnership(tags, AggregateCondition.All, false);
    }

    private ICondition IsAtLeastOneOf(params string[] tags)
    {
        return new ManyTagOwnership(tags, AggregateCondition.All, true);
    }

    public ICondition Is(string tag)
    {
        return new SingleTagOwnership(tag, true);
    }

    public ICondition IsNot(string tag)
    {
        return new SingleTagOwnership(tag, false);
    }

    private void OnMoveCompleted(MoveData moveData, MoveStatus status)
    {
        moveData.Mover.MostRecentMoveDirection = moveData.Direction;

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

    public bool CouldMoveTo(Entity mover, GridPosition position)
    {
        var data = new MoveData(
            mover,
            mover.Position,
            position,
            Direction.None);

        var status = EvaluateMove(data);
        return status.WasSuccessful;
    }
}

public readonly record struct StateValueBoolean(string Key, bool Value, bool Fallback) : ICondition
{
    public bool Check(Entity entity)
    {
        return entity.State.GetBoolOrFallback(Key, Fallback) == Value;
    }
}

public interface ICondition
{
    bool Check(Entity entity);
}

public enum AggregateCondition
{
    Any,
    All
}

public readonly record struct ManyTagOwnership(IEnumerable<string> Tags, AggregateCondition Condition, bool ShouldHave)
    : ICondition
{
    public bool Check(Entity entity)
    {
        if (Condition == AggregateCondition.All)
        {
            foreach (var tag in Tags)
            {
                if (entity.HasTag(tag) != ShouldHave)
                {
                    return false;
                }
            }

            return true;
        }

        if (Condition == AggregateCondition.Any)
        {
            foreach (var tag in Tags)
            {
                if (entity.HasTag(tag) == ShouldHave)
                {
                    return true;
                }
            }

            return false;
        }

        throw new Exception($"Unknown Aggregation Condition {Condition}");
    }
}

public readonly record struct SingleTagOwnership(string Tag, bool ShouldHave) : ICondition
{
    public bool Check(Entity entity)
    {
        return entity.HasTag(Tag) == ShouldHave;
    }
}

public readonly record struct TagComparer(Entity Mover, Entity Target)
{
    public bool Check(ICondition[] mover, ICondition[] target)
    {
        foreach (var tag in mover)
        {
            if (!tag.Check(Mover))
            {
                return false;
            }
        }

        foreach (var tag in target)
        {
            if (!tag.Check(Target))
            {
                return false;
            }
        }

        return true;
    }
}
