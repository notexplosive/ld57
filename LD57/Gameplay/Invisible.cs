using LD57.Rendering;

namespace LD57.Gameplay;

public class Invisible : IEntityAppearance
{
    public TileState TileState { get; set; } = TileState.TransparentEmpty;
    public int RawSortPriority => 0;
}
