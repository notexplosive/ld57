using ExplogineCore.Lua;
using ExplogineMonoGame.AssetManagement;
using LD57.Rendering;

namespace LD57.Tableau;

[LuaBoundType]
public readonly record struct SpriteTileInfo(SpriteSheet Sheet, int Frame) : ITileInfo
{
    public TileState GetTileState()
    {
        return TileState.Sprite(Sheet, Frame);
    }
}
