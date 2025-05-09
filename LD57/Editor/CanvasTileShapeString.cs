using LD57.Rendering;

namespace LD57.Editor;

public record CanvasTileShapeString(string StringContent) : ICanvasTileShape
{
    public TileState GetTileState()
    {
        return TileState.StringCharacter(StringContent);
    }
}
