using System.Collections.Generic;
using ExplogineMonoGame.Data;
using LD57.Gameplay.Behaviors;
using LD57.Gameplay.Triggers;
using LD57.Rendering;

namespace LD57.Gameplay;

public class Entity
{
    private readonly List<IEntityBehavior> _behaviors = new();
    private readonly int _rawSortPriority;
    private readonly HashSet<string> _tags = new();
    private bool _hasStarted;

    public Entity(World world, GridPosition position, IEntityAppearance appearance)
    {
        World = world;
        Position = position;
        Appearance = appearance;
        State.Updated += OnStateUpdated;
    }

    public Entity(World world, GridPosition position, EntityTemplate template)
        : this(world, position, template.CreateAppearance())
    {
        State.SetString("template_name", template.TemplateName);
        State.AddFromDictionary(template.State);

        foreach (var tag in template.Tags)
        {
            _tags.Add(tag);
        }

        _rawSortPriority = template.SortPriority;
    }

    public World World { get; }

    public IEntityAppearance? Appearance { get; }

    public bool IsActive { get; private set; } = true;

    public State State { get; } = new();

    public int SortPriority => _rawSortPriority * 2 + (IsActive ? 0 : 1);

    public TweenableGlyph TweenableGlyph { get; } = new();

    public TileState? TileState => Appearance?.TileState;

    public GridPosition Position { get; set; }
    public Direction MostRecentMoveDirection { get; set; } = Direction.None;

    public void Start(World world)
    {
        if (!_hasStarted)
        {
            TriggerBehavior(WorldStartTrigger.Instance);
        }

        TriggerBehavior(new SignalChangeTrigger());

        if (!world.IsEditMode)
        {
            world.Rules.WarpToPosition(this, Position);
        }

        _hasStarted = true;
    }

    private void OnStateUpdated(string key, string value)
    {
        TriggerBehavior(new StateChangeTrigger(key));
    }

    public Entity AddTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }
    
    public void RemoveTag(string tag)
    {
        _tags.Remove(tag);
    }

    public bool HasTag(string tag)
    {
        return _tags.Contains(tag);
    }

    public void SetActive(bool value)
    {
        IsActive = value;
    }

    public void AddBehavior(IEntityBehavior entityBehavior)
    {
        _behaviors.Add(entityBehavior);

        if (_hasStarted)
        {
            // if this is an OnWorldStart trigger, and we've already started, just fire it immediately
            entityBehavior.OnTrigger(this, WorldStartTrigger.Instance);
        }
    }

    public void TriggerBehavior(IBehaviorTrigger trigger)
    {
        foreach (var behavior in _behaviors)
        {
            behavior.OnTrigger(this, trigger);
        }
    }

    public void RemoveAllBehaviors()
    {
        _behaviors.Clear();
    }

    public void UnStart()
    {
        _hasStarted = false;
    }
}
