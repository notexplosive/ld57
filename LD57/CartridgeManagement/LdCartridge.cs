using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using LD57.Editor;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace LD57.CartridgeManagement;

public class LdCartridge(IRuntime runtime) : BasicGameCartridge(runtime)
{
    private EditorSession _editorSession = null!;
    private LdSession _gameSession = null!;
    private EditorSession _drawSession = null!;
    private ISession? _session;

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.PointClamp);

    public override void OnCartridgeStarted()
    {
        var targetMode = Client.Args.GetValue<string>("mode");
        var worldName = Client.Args.GetValue<string>("level");

        if (!string.IsNullOrEmpty(worldName))
        {
            HotReloadCache.LevelEditorOpenFileName ??= worldName;
        }
        else
        {
            worldName = "default";
        }

        _editorSession = BuildEditorSession();
        _drawSession = BuildDrawSession();
        _gameSession = new LdSession((Runtime.Window as RealWindow)!, Runtime.FileSystem);

        _gameSession.RequestLevelEditor += () =>
        {
            _gameSession.StopAllAmbientSounds();
            _session = _editorSession;
        };

        if (targetMode == "edit")
        {
            _session = _editorSession;
        }
        else if (targetMode == "draw")
        {
            _session = _drawSession;
        }
        else
        {
            SwitchToGameSession();
            var template = Constants.AttemptLoadWorldTemplateFromWorldDirectory(worldName);
            if (template != null)
            {
                _gameSession.StartingTemplate = template;
            }

            _gameSession.OpenMainMenu();
        }
    }

    private EditorSession BuildDrawSession()
    {
        var filter = new CanvasBrushFilter();
        var canvasSurface = new CanvasEditorSurface(filter);
        
        var editorSession = new EditorSession((Runtime.Window as RealWindow)!, Runtime.FileSystem, canvasSurface);
        editorSession.EditorTools.Add(new CanvasEditorBrushTool(editorSession, canvasSurface, filter));
        editorSession.EditorTools.Add(new CanvasSelectionTool(editorSession, canvasSurface, filter));
        editorSession.EditorTools.Add(new CanvasTextTool(canvasSurface, filter));
        editorSession.ExtraUi.Add(filter.CreateUi);
        
        editorSession.RebuildScreen();
        
        filter.RequestedModal += editorSession.OpenPopup;

        return editorSession;
    }

    private EditorSession BuildEditorSession()
    {
        EditorSelector<EntityTemplate> templateSelector = new();
        var worldEditorSurface = new WorldEditorSurface();
        var filter = new WorldEditorBrushFilter();

        var editorSession = new EditorSession((Runtime.Window as RealWindow)!, Runtime.FileSystem, worldEditorSurface);
        editorSession.EditorTools.Add(new WorldEditorBrushTool(editorSession, worldEditorSurface, filter, 
            () => templateSelector.Selected));
        editorSession.EditorTools.Add(new WorldSelectionTool(editorSession, worldEditorSurface, filter,
            () => templateSelector.Selected));
        editorSession.EditorTools.Add(new ChangeSignalTool(editorSession, worldEditorSurface));
        editorSession.EditorTools.Add(new TriggerTool(editorSession, worldEditorSurface));
        editorSession.EditorTools.Add(new PlayTool(editorSession, worldEditorSurface));
        editorSession.ExtraUi.Add(screen =>
        {
            var tilePalette = new UiElement(new GridRectangle(new GridPosition(3, 0), new GridPosition(screen.Width, 3)));
            var i = 0;
            var j = 0;

            var tempWorld = new World(new GridPosition(1, 1), new WorldTemplate(), true);
            foreach (var template in LdResourceAssets.Instance.EntityTemplates.Values)
            {
                var tempEntity = new Entity(tempWorld, new GridPosition(0, 0), template);
                tilePalette.AddSelectable(new SelectableButton<EntityTemplate>(
                    new GridPosition(1 + i, 1 + j), tempEntity.TileState,
                    templateSelector, template));

                i++;
                if (i > tilePalette.Rectangle.Width - 3)
                {
                    i = 0;
                    j++;
                }
            }

            return tilePalette;
        });

        editorSession.ExtraKeyBinds.Add((input, delta) =>
        {
            if (input.Keyboard.Modifiers.Shift)
            {
                if (templateSelector.Selected != null)
                {
                    var allTemplates = LdResourceAssets.Instance.EntityTemplates.Values.ToList();
                    var currentIndex = allTemplates.IndexOf(templateSelector.Selected);
                    var newIndex = Math.Clamp(currentIndex + delta, 0, allTemplates.Count - 1);
                    templateSelector.Selected = allTemplates[newIndex];
                }
            }
        });

        editorSession.RebuildScreen();
        
        
        worldEditorSurface.RequestedPlayAt += position =>
        {
            _gameSession.LoadWorld(worldEditorSurface.Data, position);
            SwitchToGameSession();
        };

        return editorSession;
    }

    private void SwitchToGameSession()
    {
        _session = _gameSession;
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _session?.UpdateInput(input, hitTestStack);
    }

    public override void Update(float dt)
    {
        _session?.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _session?.Draw(painter);
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
        parameters.RegisterParameter<string>("mode");
        parameters.RegisterParameter<string>("level");
    }

    public override void OnHotReload()
    {
        _session?.OnHotReload();
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
            
            var texture = ResourceAlias.PopupFrameRaw.SourceTexture;
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
