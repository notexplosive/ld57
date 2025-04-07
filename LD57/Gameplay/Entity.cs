using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;
using LD57.Rendering;

namespace LD57.Gameplay;

public class Entity
{
    private readonly List<EntityBehavior> _behaviors = new();
    private readonly int _rawSortPriority;
    private readonly HashSet<string> _tags = new();
    private bool _hasStarted;

    public Entity(GridPosition position, IEntityAppearance appearance)
    {
        Position = position;
        Appearance = appearance;
        State.Updated += OnStateUpdated;
    }

    public Entity(GridPosition position, EntityTemplate template)
        : this(position, template.SpriteSheetName != null
            ? new EntityAppearance(LdResourceAssets.Instance.Sheets[template.SpriteSheetName],
                template.Frame,
                ResourceAlias.Color(template.Color))
            : new Invisible())
    {
        State.AddFromDictionary(template.State);

        foreach (var tag in template.Tags)
        {
            _tags.Add(tag);
        }

        _rawSortPriority = template.SortPriority;
    }

    public IEntityAppearance? Appearance { get; }

    public bool IsActive { get; private set; } = true;

    public State State { get; } = new();

    public int SortPriority => _rawSortPriority * 2 + (IsActive ? 0 : 1);

    public TweenableGlyph TweenableGlyph { get; } = new();

    public TileState? TileState => Appearance?.TileState;

    public GridPosition Position { get; set; }
    public Direction MostRecentMoveDirection { get; set; } = Direction.None;

    public void Start()
    {
        SelfTriggerBehavior(BehaviorTrigger.OnWorldStart);
        SelfTriggerBehavior(BehaviorTrigger.OnSignalChange);
        SelfTriggerBehaviorWithPayload(BehaviorTrigger.OnEntityMoved, new BehaviorTriggerWithEntity.Payload(this));
        _hasStarted = true;
    }

    private void OnStateUpdated(string key, string value)
    {
        SelfTriggerBehaviorWithPayload(BehaviorTrigger.OnStateChanged,
            new BehaviorTriggerWithKeyValuePair.Payload(key, value));
    }

    public Entity AddTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public bool HasTag(string tag)
    {
        return _tags.Contains(tag);
    }

    public void SetActive(bool value)
    {
        IsActive = value;
    }

    private void AddBehavior(EntityBehavior entityBehavior)
    {
        if (_hasStarted)
        {
            // if this is an OnWorldStart trigger, and we've already started, just fire it immediately
            if (entityBehavior.Trigger == BehaviorTrigger.OnWorldStart)
            {
                entityBehavior.DoActionEmptyPayload();
            }
        }

        _behaviors.Add(entityBehavior);
    }

    public void AddBehavior(BehaviorTriggerBasic basicTrigger, Action result)
    {
        AddBehavior(new EntityBehavior(basicTrigger, _ => { result(); }));
    }

    public void AddBehavior<TPayload>(IBehaviorTrigger<TPayload> behaviorTriggerWithString, Action<TPayload> result)
        where TPayload : IBehaviorTriggerPayload
    {
        AddBehavior(new EntityBehavior(behaviorTriggerWithString, payload =>
        {
            if (payload is TPayload realPayload)
            {
                result(realPayload);
            }
            else
            {
                result(behaviorTriggerWithString.CreateEmptyPayload());
            }
        }));
    }

    public void SelfTriggerBehavior(BehaviorTriggerBasic trigger)
    {
        foreach (var behavior in _behaviors.Where(behavior => behavior.Trigger == trigger))
        {
            behavior.DoActionEmptyPayload();
        }
    }

    public void SelfTriggerBehaviorWithPayload<TPayload>(IBehaviorTrigger<TPayload> trigger, TPayload payload)
        where TPayload : IBehaviorTriggerPayload
    {
        foreach (var behavior in _behaviors.Where(behavior => behavior.Trigger == trigger))
        {
            behavior.DoAction(payload);
        }
    }
}
