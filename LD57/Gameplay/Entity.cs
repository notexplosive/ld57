using System.Collections.Generic;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;
using LD57.Rendering;

namespace LD57.Gameplay;

public class Entity
{
    private readonly IEntityAppearance? _appearance;
    private readonly HashSet<string> _tags = new();
    private readonly int _rawSortPriority;
    public bool IsActive { get; private set; } = true;

    public Entity(GridPosition position, IEntityAppearance appearance)
    {
        Position = position;
        _appearance = appearance;
    }

    public Entity(GridPosition position, EntityTemplate template)
    {
        Position = position;
        _appearance = new EntityAppearance(LdResourceAssets.Instance.Sheets[template.SpriteSheetName], template.Frame, ResourceAlias.Color(template.Color));
        foreach (var tag in template.Tags)
        {
            _tags.Add(tag);
        }

        _rawSortPriority = template.SortPriority;
    }

    public int SortPriority => _rawSortPriority * 2 + (IsActive ? 0 : 1);

    public TweenableGlyph TweenableGlyph { get; } = new();

    public TileState TileState
    {
        get
        {
            if (_appearance == null)
            {
                return TileState.Empty;
            }

            return _appearance.TileState;
        }
    }

    public GridPosition Position { get; set; }

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
}
