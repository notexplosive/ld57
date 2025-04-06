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
        var status = new MoveStatus(data);

        var targets = _world.GetActiveEntitiesAt(data.Destination).ToList();

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
                _world.Destroy(target);
                mover.SetActive(false);
            }

            if (tags.Check(
                    [Is("FillsWater"), IsNot("FloatsInWater")],
                    [Is("Water")]))
            {
                _world.Destroy(target);
                _world.Destroy(mover);
            }
        }

        if (status.WasSuccessful)
        {
            mover.Position += new GridPosition(direction.ToPoint());
        }

        OnMoveCompleted(data, status);
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

    public void WarpToPosition(Entity entity, GridPosition newPosition)
    {
        var status = new MoveStatus
        {
            MoveData = new MoveData
            {
                Destination = newPosition,
                Direction = Direction.None,
                Mover = entity,
                Source = entity.Position
            }
        };

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
