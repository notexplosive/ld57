using LD57.Rendering;

namespace LD57.Editor;

public class CanvasTileShapeString : ICanvasTileShape
{
    private readonly string _stringContent;

    public CanvasTileShapeString(string stringContent)
    {
        _stringContent = stringContent;
    }

    public TileState GetTileState()
    {
        return TileState.StringCharacter(_stringContent);
    }
}
