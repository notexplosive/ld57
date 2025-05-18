using System.IO;
using System.Linq;
using ExplogineCore;
using ExplogineCore.Lua;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Tableau;

public class TableauSession : ISession
{
    private readonly AsciiScreen _screen = new(0, 0, 1);
    private bool _fileHasChanged;
    private string _fileName = string.Empty;
    private FileSystemWatcher? _fileWatcher;
    private LuaRuntime _luaRuntime = null!;
    private Tableau? _tableau;

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
        
        if (input.Keyboard.Modifiers.Control &&input.Keyboard.GetButton(Keys.O, true).WasPressed)
        {
            var fileName = PlatformFileApi.OpenFileDialogue("Open Tableau",
                new PlatformFileApi.ExtensionDescription("lua", "Tableau Lua"));
            if (fileName != null)
            {
                OpenFile(fileName);
            }
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
            lines.Insert(0, _luaRuntime.CurrentError.Exception.Message);
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                _screen.PutString(new GridPosition(0, i), line);
            }

            return;
        }

        _screen.Clear(TileState.TransparentEmpty);

        if (_tableau == null)
        {
            return;
        }

        _tableau.Update(dt);
    }

    public void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
        _screen.Draw(painter, Vector2.Zero);
    }

    public AsciiScreen Screen => _screen;

    public void OpenFile(string fileName)
    {
        var fileSystem = Client.Debug.RepoFileSystem.GetDirectory("Resource/Tableaus");
        _luaRuntime = new LuaRuntime(fileSystem);

        var ipsum = new LuaIpsum(_luaRuntime, _screen);

        // execute lua script with ipsum set
        _luaRuntime.SetGlobal("ipsum", ipsum);
        _luaRuntime.SetGlobal("print", (object) Client.Debug.Log);
        _luaRuntime.DoFile(fileName);

        // setup settings
        _tableau = new Tableau(_screen, _luaRuntime, ipsum);


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
