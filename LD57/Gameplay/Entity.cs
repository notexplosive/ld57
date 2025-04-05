using LD57.Rendering;

namespace LD57.Gameplay;

public class Entity
{
    private EntityAppearance? Appearance;

    public Entity(GridPosition position, EntityAppearance appearance)
    {
        Position = position;
        Appearance = appearance;
    }

    public TileState TileState
    {
        get
        {
            if (Appearance == null)
            {
                return TileState.Empty;
            }

            return Appearance.TileState;
        }
    }

    public GridPosition Position { get; set; }
}
