using System;
using System.Linq;
using ExplogineCore.Lua;
using ExplogineMonoGame;
using JetBrains.Annotations;
using LD57.Editor;
using LD57.Rendering;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

namespace LD57.Tableau;

[LuaBoundType]
public class LuaIpsum
{
    private readonly LuaRuntime _luaRuntime;
    private readonly AsciiScreen _screen;

    public LuaIpsum(LuaRuntime luaRuntime, AsciiScreen screen)
    {
        _luaRuntime = luaRuntime;
        _screen = screen;
    }

    [LuaMember("update")]
    public Closure? Update { get; set; }

    [LuaMember("setup")]
    public Closure? Setup { get; set; }

    [LuaMember("width")]
    public int Width { get; set; }

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

    [UsedImplicitly]
    [LuaMember("putImage")]
    public void PutImage(TileImage tileImage, int x, int y)
    {
        tileImage.Draw(_screen, new GridPosition(x,y));
    }

    [UsedImplicitly]
    [LuaMember("loadImage")]
    public TileImage LoadImage(string path)
    {
        return new TileImage(path);
    }
}

[LuaBoundType]
public record TileImage
{
    private readonly CanvasData _data;

    public TileImage(string fileName)
    {
        var file = Client.Debug.RepoFileSystem.GetDirectory("Resource/Canvases").ReadFile(fileName + ".json");
        _data = JsonConvert.DeserializeObject<CanvasData>(file) ??
                throw new Exception($"Could not load image: {fileName}");
    }

    public void Draw(AsciiScreen screen, GridPosition gridPosition)
    {
        screen.PushTransform(gridPosition);

        foreach (var tile in _data.Content)
        {
            screen.PutTile(tile.Position, tile.CanvasTileData.GetTileUnfiltered());
        }
        
        screen.PopTransform();
    }
}
