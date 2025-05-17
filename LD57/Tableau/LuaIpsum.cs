using System;
using System.Linq;
using ExplogineCore.Lua;
using JetBrains.Annotations;
using LD57.Rendering;
using MoonSharp.Interpreter;

namespace LD57.Tableau;

[LuaBoundType]
public class LuaIpsum
{
    private readonly LuaRuntime _luaRuntime;
    private readonly AsciiScreen _screen;

    [LuaMember("update")]
    public Closure? Update { get; set; }
    
    [LuaMember("setup")]
    public Closure? Setup { get; set; }

    [LuaMember("width")]
    public int Width { get; set; }

    public LuaIpsum(LuaRuntime luaRuntime, AsciiScreen screen)
    {
        _luaRuntime = luaRuntime;
        _screen = screen;
    }

    [UsedImplicitly]
    [LuaMember("sprite")]
    public SpriteTileInfo CreateSprite(string sheetName, int frame)
    {

        var sheet = ResourceAlias.GetSpriteSheetByName(sheetName);

        if (sheet == null)
        {
            throw new Exception($"Could not load sprite sheet: `{sheetName}`");
        }
        
        return new SpriteTileInfo(sheet, frame);
    }
    
    [UsedImplicitly]
    [LuaMember("character")]
    public CharacterTileInfo CreateCharacter(string text)
    {
        return new CharacterTileInfo(text.First().ToString());
    }
    
    [UsedImplicitly]
    [LuaMember("putTile")]
    public void PutTile(ITileInfo tileInfo, int x, int y)
    {
        _screen.PutTile(new GridPosition(x, y), tileInfo.GetTileState());
    }
}
