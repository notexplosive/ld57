using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasTileShapeEmpty : ICanvasTileShape
{
    public TileState GetTileState()
    {
        return TileState.BackgroundOnly(Color.Transparent, 0f);
    }
}
