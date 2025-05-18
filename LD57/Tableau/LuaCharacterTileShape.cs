using ExplogineCore.Lua;
using LD57.Rendering;

namespace LD57.Tableau;

[LuaBoundType]
public readonly record struct LuaCharacterTileShape(string Text) : ILuaTileShape
{
    public TileState GetTileState()
    {
        return TileState.StringCharacter(Text);
    }
}
