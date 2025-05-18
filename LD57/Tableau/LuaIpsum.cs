using System.Linq;
using ExplogineCore.Data;
using ExplogineCore.Lua;
using ExplogineMonoGame;
using JetBrains.Annotations;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace LD57.Tableau;

[LuaBoundType]
public class LuaIpsum
{
    private readonly LuaRuntime _luaRuntime;
    private readonly AsciiScreen _screen;
    private Color _backgroundColor;
    private float _backgroundIntensity;
    private Color _color = ResourceAlias.Color("default");
    private TileTransform _transform = new(0, false, false);
    private readonly int _seed;

    public LuaIpsum(LuaRuntime luaRuntime, AsciiScreen screen)
    {
        _luaRuntime = luaRuntime;
        _screen = screen;
        _seed = Client.Random.Clean.NextInt();
    }

    [LuaMember("update")]
    public Closure? Update { get; set; }

    [LuaMember("setup")]
    public Closure? Setup { get; set; }

    [LuaMember("width")]
    public int Width { get; set; }

    [UsedImplicitly]
    [LuaMember("sprite")]
    public LuaSpriteTileShape CreateSprite(string sheetName, int frame)
    {
        return new LuaSpriteTileShape(sheetName, frame, _transform.Angle, _transform.FlipX, _transform.FlipY);
    }

    [UsedImplicitly]
    [LuaMember("character")]
    public LuaCharacterTileShape CreateCharacter(string text)
    {
        return new LuaCharacterTileShape(text.First().ToString());
    }

    [UsedImplicitly]
    [LuaMember("putTile")]
    public void PutTile(ILuaTileShape tileShapeInfo, float x, float y)
    {
        _screen.PutTile(new GridPosition(Constants.RoundToInt(x), Constants.RoundToInt(y)), tileShapeInfo.GetTileState()
            with
            {
                ForegroundColor = _color,
                BackgroundIntensity = _backgroundIntensity,
                BackgroundColor = _backgroundColor,
            }
        );
    }

    [UsedImplicitly]
    [LuaMember("putImage")]
    public void PutImage(LuaTileImage image, float x, float y)
    {
        image.Draw(_screen, new GridPosition(Constants.RoundToInt(x), Constants.RoundToInt(y)));
    }

    [UsedImplicitly]
    [LuaMember("loadImage")]
    public LuaTileImage LoadImage(string path)
    {
        return new LuaTileImage(path, _luaRuntime);
    }

    [UsedImplicitly]
    [LuaMember("setColor")]
    public void SetColor(string colorName)
    {
        _color = ResourceAlias.Color(colorName);
    }

    [UsedImplicitly]
    [LuaMember("setBackgroundColor")]
    public void SetBackgroundColor(string colorName)
    {
        _backgroundColor = ResourceAlias.Color(colorName);
    }

    [UsedImplicitly]
    [LuaMember("setBackgroundIntensity")]
    public void SetBackgroundIntensity(float intensity)
    {
        _backgroundIntensity = intensity;
    }

    [UsedImplicitly]
    [LuaMember("seed")]
    public int Seed()
    {
        return _seed;
    }
    
    [UsedImplicitly]
    [LuaMember("noise")]
    public LuaNoise CreateNoise(int? seed)
    {
        return new LuaNoise(new Noise(seed ?? _seed));
    }
}