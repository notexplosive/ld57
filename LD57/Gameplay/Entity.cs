using System.Collections.Generic;
using System.Linq;
using LD57.CartridgeManagement;
using LD57.Rendering;

namespace LD57.Gameplay;

public class Entity
{
    public IEntityAppearance? Appearance { get; }
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
        : this(position, new EntityAppearance(LdResourceAssets.Instance.Sheets[template.SpriteSheetName],
            template.Frame,
            ResourceAlias.Color(template.Color)))
    {
        State.AddFromDictionary(template.State);
        
        foreach (var tag in template.Tags)
        {
            _tags.Add(tag);
        }
        
        _rawSortPriority = template.SortPriority;
    }

    public bool IsActive { get; private set; } = true;

    public State State { get; } = new();

    public int SortPriority => _rawSortPriority * 2 + (IsActive ? 0 : 1);

    public TweenableGlyph TweenableGlyph { get; } = new();

    public TileState? TileState => Appearance?.TileState;

    public GridPosition Position { get; set; }

    public void Start()
    {
        TriggerBehavior(BehaviorTrigger.OnWorldStart);
        
        _hasStarted = true;
    }
    
    private void OnStateUpdated(string key, string value)
    {
        TriggerBehavior(BehaviorTrigger.OnStateChanged);
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

    public void AddBehavior(EntityBehavior entityBehavior)
    {
        if (_hasStarted)
        {
            if (entityBehavior.Trigger == BehaviorTrigger.OnWorldStart)
            {
                entityBehavior.DoAction();
            }
        }

        _behaviors.Add(entityBehavior);
    }

    public void TriggerBehavior(BehaviorTrigger trigger)
    {
        foreach (var behavior in _behaviors.Where(behavior => behavior.Trigger == trigger))
        {
            behavior.DoAction();
        }
    }
}
