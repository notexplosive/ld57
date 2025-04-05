using System.Collections.Generic;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;
using LD57.Rendering;

namespace LD57.Gameplay;

public class Entity
{
    private readonly IEntityAppearance? _appearance;
    private readonly HashSet<string> _tags = new();

    public Entity(GridPosition position, IEntityAppearance appearance)
    {
        Position = position;
        _appearance = appearance;
    }

    public Entity(GridPosition position, EntityTemplate template)
    {
        Position = position;
        _appearance = new EntityAppearance(LdResourceAssets.Instance.Sheets[template.SpriteSheetName], template.Frame, ColorExtensions.FromRgbaHexString(template.ColorHex));
        foreach (var tag in template.Tags)
        {
            _tags.Add(tag);
        }
    }

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
}
