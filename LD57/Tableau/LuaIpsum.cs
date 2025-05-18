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
    private readonly int _seed;
    private readonly TileTransform _transform = new(0, false, false);
    private Color _backgroundColor;
    private float _backgroundIntensity;
    private Color _color = ResourceAlias.Color("default");

    public LuaIpsum(LuaRuntime luaRuntime, AsciiScreen screen)
    {
        _luaRuntime = luaRuntime;
        _screen = screen;
        _seed = Client.Random.Clean.NextInt();
    }

    public int DesiredWidth { get; private set; }

    [LuaMember("update")]
    public Closure? Update { get; set; }

    [LuaMember("setup")]
    public Closure? Setup { get; set; }

    [UsedImplicitly]
    [LuaMember("width")]
    public int Width()
    {
        // Assume that desired width is already set
        return DesiredWidth;
    }

    [UsedImplicitly]
    [LuaMember("height")]
    public int Height()
    {
        return _screen.Height - 1;
    }

    [UsedImplicitly]
    [LuaMember("setWidth")]
    public void SetWidth(int width)
    {
        DesiredWidth = width;
    }

    [UsedImplicitly]
    [LuaMember("sprite")]
    public LuaSpriteTileShape CreateSprite(string sheetName, int frame, float angle = 0, bool flipX = false,
        bool flipY = false)
    {
        return new LuaSpriteTileShape(sheetName, frame, angle, flipX, flipY);
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
                BackgroundColor = _backgroundColor
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
    public void SetColor(string colorName, string? backgroundColor = null, float? backgroundIntensity = null)
    {
        _color = ResourceAlias.Color(colorName);
        _backgroundIntensity = 0;

        if (backgroundColor != null)
        {
            _backgroundColor = ResourceAlias.Color(backgroundColor);
            _backgroundIntensity = backgroundIntensity ?? 1;
        }
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

    [UsedImplicitly]
    [LuaMember("allColors")]
    public Table AllColors()
    {
        var result = _luaRuntime.NewTable();

        foreach (var color in LdResourceAssets.Instance.NamedColors.Keys)
        {
            result.Append(DynValue.NewString(color));
        }

        return result;
    }

    [UsedImplicitly]
    [LuaMember("allSheets")]
    public Table AllSheets()
    {
        var result = _luaRuntime.NewTable();

        foreach (var (sheetName, sheet) in LdResourceAssets.Instance.AllNamedSheets())
        {
            result.Append(DynValue.NewString(sheetName));
        }

        return result;
    }

    [UsedImplicitly]
    [LuaMember("framesInSheet")]
    public int FramesInSheet(string sheetName)
    {
        return ResourceAlias.GetSpriteSheetByName(sheetName)?.FrameCount ?? 0;
    }
}
