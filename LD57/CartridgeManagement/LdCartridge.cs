using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineCore;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using LD57.Core;
using LD57.Editor;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace LD57.CartridgeManagement;

public class LdCartridge(IRuntime runtime) : BasicGameCartridge(runtime)
{
    private EditorSession _drawSession = null!;
    private EditorSession _editorSession = null!;
    private LdSession _gameSession = null!;
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
        var chords = new List<KeybindChord>();
        var filter = new CanvasBrushFilter();
        var canvasSurface = new CanvasEditorSurface(filter);
        var toolChord = new KeybindChord(Keys.Q, "Tools");
        chords.Add(toolChord);

        var editorSession =
            new EditorSession((Runtime.Window as RealWindow)!, Runtime.FileSystem, canvasSurface, chords);
        editorSession.AddTool(
            toolChord, Keys.R, "Brush", new CanvasEditorBrushTool(editorSession, canvasSurface, filter));
        editorSession.AddTool(
            toolChord, Keys.S, "Selection", new CanvasSelectionTool(editorSession, canvasSurface, filter));
        var eyeDropper = editorSession.AddTool(
            toolChord, Keys.E, "Eye Dropper", new CanvasEyeDropperTool(editorSession, canvasSurface, filter));
        editorSession.AddTool(
            toolChord, Keys.T, "Text", new CanvasTextTool(canvasSurface, filter));
        editorSession.ExtraUi.Add(filter.CreateUi);

        chords.Add(new KeybindChord(Keys.E, "Brush Filter")
            .Add(Keys.S, "Shape", true, screen => filter.OpenShapeModal(screen))
            .Add(Keys.D, "Foreground Color", true, screen => filter.OpenForegroundColorModal(screen))
            .Add(Keys.F, "Background Color", true, screen => filter.OpenBackgroundColorModal(screen))
        );

        chords.Add(new KeybindChord(Keys.T, "Transform")
            .AddDynamicTile(ChooseShapeModal.GetMirrorHorizontallyTile(() => filter.FlipState, () => filter.Rotation))
            .AddDynamicTile(ChooseShapeModal.GetMirrorVerticallyTile(() => filter.FlipState, () => filter.Rotation))
            .AddDynamicTile(ChooseShapeModal.GetCurrentRotationTile(() => filter.Rotation))
            .Add(Keys.R, "Reset Transform", false, screen =>
            {
                filter.Rotation = QuarterRotation.Upright;
                filter.FlipState = XyBool.False;
            })
            .Add(Keys.H, "Flip Horizontal", false, screen =>
            {
                if (filter.Rotation == QuarterRotation.Upright || filter.Rotation == QuarterRotation.Half)
                {
                    filter.FlipState = filter.FlipState with {X = !filter.FlipState.X};
                }
                else
                {
                    filter.FlipState = filter.FlipState with {Y = !filter.FlipState.Y};
                }
            })
            .Add(Keys.V, "Flip Vertical", false, screen =>
            {
                if (filter.Rotation == QuarterRotation.Upright || filter.Rotation == QuarterRotation.Half)
                {
                    filter.FlipState = filter.FlipState with {Y = !filter.FlipState.Y};
                }
                else
                {
                    filter.FlipState = filter.FlipState with {X = !filter.FlipState.X};
                }
            })
            .Add(Keys.Q, "Rotate CCW", false, screen => filter.Rotation = filter.Rotation.CounterClockwisePrevious())
            .Add(Keys.E, "Rotate CW", false, screen => filter.Rotation = filter.Rotation.ClockwiseNext())
        );

        editorSession.RebuildScreen();

        filter.RequestedModal += editorSession.OpenPopup;
        canvasSurface.RequestedEyeDropper += eyeDropper.GrabTile;

        return editorSession;
    }

    private EditorSession BuildEditorSession()
    {
        var templateSelector = new EditorSelector<EntityTemplate>();
        var chords = new List<KeybindChord>();
        var worldEditorSurface = new WorldEditorSurface();
        var filter = new WorldEditorBrushFilter();

        var toolChord = new KeybindChord(Keys.Q, "Tools");
        chords.Add(toolChord);

        var editorSession = new EditorSession((Runtime.Window as RealWindow)!, Runtime.FileSystem, worldEditorSurface,
            chords);
        editorSession.AddTool(
            toolChord, Keys.B, "Brush",
            new WorldEditorBrushTool(editorSession, worldEditorSurface, filter, () => templateSelector.Selected));
        editorSession.AddTool(
            toolChord, Keys.S, "Selection", new WorldSelectionTool(editorSession, worldEditorSurface, filter,
                () => templateSelector.Selected));
        editorSession.AddTool(toolChord, Keys.G, "Signal", new ChangeSignalTool(editorSession, worldEditorSurface));
        editorSession.AddTool(toolChord, Keys.T, "Trigger", new TriggerTool(editorSession, worldEditorSurface));
        editorSession.AddTool(toolChord, Keys.P, "Play", new PlayTool(editorSession, worldEditorSurface));
        editorSession.ExtraUi.Add(screen =>
        {
            var tilePalette =
                new UiElement(new GridRectangle(new GridPosition(3, 0), new GridPosition(screen.Width, 3)));
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
