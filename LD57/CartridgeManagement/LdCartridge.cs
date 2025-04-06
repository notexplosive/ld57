using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using LD57.Gameplay;
using LD57.Sessions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace LD57.CartridgeManagement;

public class LdCartridge(IRuntime runtime) : BasicGameCartridge(runtime)
{
    private EditorSession _editorSession = null!;
    private LdSession _gameSession = null!;
    private ISession _session = null!;

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.PointClamp);

    public override void OnCartridgeStarted()
    {
        _editorSession = new EditorSession((Runtime.Window as RealWindow)!, Runtime.FileSystem);
        _gameSession = new LdSession((Runtime.Window as RealWindow)!, Runtime.FileSystem);

        _editorSession.RequestPlay += (position) =>
        {
            _gameSession.LoadWorld(_editorSession.WorldTemplate, position);
            _session = _gameSession;
        };

        var targetMode = Client.Args.GetValue<string>("mode");
        if (targetMode == "play")
        {
            LoadGame();
        }
        else if (targetMode == "editor")
        {
            _session = _editorSession;
        }
        else if (Client.Debug.IsPassiveOrActive)
        {
            _session = _editorSession;
        }
        else
        {
            LoadGame();
        }
    }

    private void LoadGame()
    {
        // _gameSession.LoadCurrentLevel();
        _session = _gameSession;
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.F4).WasPressed)
            {
                ToggleSession();
            }
        }

        _session.UpdateInput(input, hitTestStack);
    }

    private void ToggleSession()
    {
        if (_session == _editorSession)
        {
            _gameSession.LoadWorld(_editorSession.WorldTemplate);
            _session = _gameSession;
        }
        else
        {
            _session = _editorSession;
        }
    }

    public override void Update(float dt)
    {
        _session.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _session.Draw(painter);
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
        parameters.RegisterParameter<string>("mode");
    }

    public override void OnHotReload()
    {
        _session.OnHotReload();
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        LdResourceAssets.Reset();
        foreach (var item in LdResourceAssets.Instance.LoadEvents(painter))
        {
            yield return item;
        }

        yield return new VoidLoadEvent("Colors", () =>
        {
            var fileSystem = Client.Debug.RepoFileSystem.GetDirectory("Resource");
            var colorTable =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(fileSystem.ReadFile("colors.json"));
            if (colorTable != null)
            {
                LdResourceAssets.Instance.AddKnownColors(colorTable);
            }
        });

        yield return new VoidLoadEvent("PopupFrameParts", "Graphics", () =>
        {
            var texture = LdResourceAssets.Instance.Sheets["PopupFrame"].SourceTexture;
            var selectFrameSpriteSheet = new SelectFrameSpriteSheet(texture);
            var tileSize = 11;
            var tileSizeSquare = new Point(tileSize);
            selectFrameSpriteSheet.AddFrame(new Rectangle(Point.Zero, tileSizeSquare));
            selectFrameSpriteSheet.AddFrame(new Rectangle(new Point(tileSize, 0), tileSizeSquare));
            selectFrameSpriteSheet.AddFrame(new Rectangle(new Point(tileSize * 2, 0), tileSizeSquare));
            selectFrameSpriteSheet.AddFrame(new Rectangle(new Point(tileSize * 2, tileSize), tileSizeSquare));
            selectFrameSpriteSheet.AddFrame(new Rectangle(new Point(tileSize * 2, tileSize * 2), tileSizeSquare));
            selectFrameSpriteSheet.AddFrame(new Rectangle(new Point(tileSize, tileSize * 2), tileSizeSquare));
            selectFrameSpriteSheet.AddFrame(new Rectangle(new Point(0, tileSize * 2), tileSizeSquare));
            selectFrameSpriteSheet.AddFrame(new Rectangle(new Point(0, tileSize), tileSizeSquare));
            LdResourceAssets.Instance.AddSpriteSheet("PopupFrameParts", selectFrameSpriteSheet);
        });

        yield return new VoidLoadEvent("Entities", () =>
        {
            var fileSystem = Client.Debug.RepoFileSystem.GetDirectory("Resource/Entities");
            foreach (var path in fileSystem.GetFilesAt("."))
            {
                var template = JsonConvert.DeserializeObject<EntityTemplate>(fileSystem.ReadFile(path));
                if (template != null)
                {
                    var key = path.RemoveFileExtension();
                    template.TemplateName = key;
                    LdResourceAssets.Instance.EntityTemplates.Add(key, template);
                }
            }
        });
        
        yield return new VoidLoadEvent("Messages", () =>
        {
            var fileSystem = Client.Debug.RepoFileSystem.GetDirectory("Resource/Messages");
            foreach (var path in fileSystem.GetFilesAt("."))
            {
                var content = fileSystem.ReadFile(path);
                {
                    var key = path.RemoveFileExtension();
                    LdResourceAssets.Instance.Messages.Add(key, new MessageContent(content));
                }
            }
        });
    }
}
