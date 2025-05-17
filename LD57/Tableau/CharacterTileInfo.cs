using ExplogineCore.Lua;
using LD57.Rendering;

namespace LD57.Tableau;

[LuaBoundType]
public readonly record struct CharacterTileInfo(string Text) : ITileInfo
{
    public TileState GetTileState()
    {
        return TileState.StringCharacter(Text);
    }
}
