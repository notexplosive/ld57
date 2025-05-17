using System;
using System.IO;
using System.Linq;
using ExplogineCore;
using ExplogineCore.Lua;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;

namespace LD57.Tableau;

public class TableauSession : ISession
{
    private readonly AsciiScreen _screen = new(0, 0, 1);
    private bool _fileHasChanged;
    private string _fileName = string.Empty;
    private FileSystemWatcher? _fileWatcher;
    private LuaRuntime _luaRuntime = null!;
    private TableauSettings? _settings;

    public TableauSession()
    {
        OpenFile("default.lua");
    }

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.GetButton(Keys.R).WasPressed)
        {
            OpenFile(_fileName);
        }
    }

    public void Update(float dt)
    {
        if (_fileHasChanged)
        {
            OpenFile(_fileName);
            _fileHasChanged = false;
        }
        
        
        if (_luaRuntime.CurrentError != null)
        {
            var lines = _luaRuntime.Callstack().SplitLines().ToList();
            lines.Insert(0,_luaRuntime.CurrentError.Exception.Message);
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                _screen.PutString(new GridPosition(0, i), line);
            }
        
            return;
        }
        
        _screen.Clear(TileState.TransparentEmpty);
        
        if (_settings == null)
        {
            return;
        }
        
        _settings.Update(dt);
    }

    public void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
        _screen.Draw(painter, Vector2.Zero);
    }

    public void OpenFile(string fileName)
    {
        var fileSystem = Client.Debug.RepoFileSystem.GetDirectory("Resource/Tableaus");
        _luaRuntime = new LuaRuntime(fileSystem);

        var ipsumObject = new LuaIpsum(_luaRuntime, _screen);
        var ipsumTable = ipsumObject.BuildLuaTable();

        // execute lua script with ipsum set
        _luaRuntime.SetGlobal("ipsum", ipsumTable);
        _luaRuntime.SetGlobal("print", Client.Debug.Log);
        _luaRuntime.DoFile(fileName);

        // setup settings
        _settings = new TableauSettings();
        _settings.SetupFromLua(_luaRuntime, ipsumTable);

        // setup screen
        _screen.SetWidth(_settings.Width);

        if (_fileName != fileName)
        {
            _fileName = fileName;
            if (_fileWatcher != null)
            {
                _fileWatcher.Dispose();
            }

            _fileWatcher = new FileSystemWatcher(fileSystem.GetCurrentDirectory(), "*.lua");
            _fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.EnableRaisingEvents = true;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.Name == _fileName)
        {
            _fileHasChanged = true;
        }
    }
}

public class LuaIpsum
{
    private readonly LuaRuntime _luaRuntime;
    private readonly AsciiScreen _screen;

    public LuaIpsum(LuaRuntime luaRuntime, AsciiScreen screen)
    {
        _luaRuntime = luaRuntime;
        _screen = screen;
    }

    public Table BuildLuaTable()
    {
        var table = _luaRuntime.NewTable();
        table["putTile"] = (object) PutTile;
        table["sprite"] = (object) GetSpriteTile;
        table["character"] = (object) GetCharacterTile;
        return table;
    }

    private SpriteTileInfo GetSpriteTile(string sheetName, int frame)
    {

        var sheet = ResourceAlias.GetSpriteSheetByName(sheetName);

        if (sheet == null)
        {
            throw new Exception($"Could not load sprite sheet: `{sheetName}`");
        }
        
        return new SpriteTileInfo(sheet, frame);
    }
    
    private CharacterTileInfo GetCharacterTile(string text)
    {
        return new CharacterTileInfo(text.First().ToString());
    }
    
    private void PutTile(ITileInfo tileInfo, int x, int y)
    {
        _screen.PutTile(new GridPosition(x, y), tileInfo.GetTileState());
    }
}

public interface ITileInfo
{
    TileState GetTileState();
}

[LuaBoundType]
public readonly record struct SpriteTileInfo(SpriteSheet Sheet, int Frame) : ITileInfo
{
    public TileState GetTileState()
    {
        return TileState.Sprite(Sheet, Frame);
    }
}

[LuaBoundType]
public readonly record struct CharacterTileInfo(string Text) : ITileInfo
{
    public TileState GetTileState()
    {
        return TileState.StringCharacter(Text);
    }
}

public class TableauSettings
{
    private Func<float, DynValue>? _updateFunction;
    public int Width { get; private set; } = 80;

    public void SetupFromLua(LuaRuntime luaRuntime, Table ipsum)
    {
        _updateFunction = dt => luaRuntime.SafeCallKeyOnTable(ipsum, "update", dt);

        // Run setup
        luaRuntime.SafeCallKeyOnTable(ipsum, "setup");

        Width = (int) (ipsum.Get("width").CastToNumber() ?? Width);
    }

    public void Update(float dt)
    {
        _updateFunction?.Invoke(dt);
    }
}
