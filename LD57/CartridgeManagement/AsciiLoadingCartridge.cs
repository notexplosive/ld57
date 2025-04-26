using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD57.CartridgeManagement;

public class AsciiLoadingCartridge : Cartridge
{
    private const string LoadingGlyphs = "/-\\|";
    private readonly int _heightLimit;
    private readonly Loader _loader;
    private readonly AsciiScreen _screen;
    private int _startingDelayFrames = 10;
    private readonly List<string> _statuses = new();

    public AsciiLoadingCartridge(IRuntime runtime, Loader loader) : base(runtime)
    {
        _loader = loader;

        _loader.BeforeLoadItem += BeforeLoadItem;
        _loader.AfterLoadItem += AfterLoadItem;

        _loader.ForceLoadVoid("Colors");
        _loader.ForceLoadVoid("Fonts");
        _loader.ForceLoadVoid("sprite-atlas");

        _screen = Constants.CreateGameScreen();

        _heightLimit = _screen.Height;

        _statuses.Add("Loading...");
    }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.PointClamp);

    private void BeforeLoadItem()
    {
        _statuses.Add(_loader.NextStatus);

        if (_statuses.Count > _heightLimit)
        {
            _statuses.RemoveAt(0);
        }
    }

    private void AfterLoadItem()
    {
    }

    public override void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
        _screen.Draw(painter, new Vector2(0, _screen.TileSize / 4f));
    }

    public override void Update(float dt)
    {
        _screen.Clear(TileState.Empty);

        var lineNumber = 0;
        string? mostRecentStatus = null;
        foreach (var status in _statuses)
        {
            if (status != mostRecentStatus)
            {
                _screen.PutString(new GridPosition(0, lineNumber), status);
                lineNumber++;
            }

            mostRecentStatus = status;
        }

        var index = (int) (Client.TotalElapsedTime * 10f % LoadingGlyphs.Length);
        _screen.PutTile(new GridPosition(_statuses.Last().Length, lineNumber - 1),
            TileState.StringCharacter(LoadingGlyphs[index].ToString()));

        LoadNextChunk();
    }

    private void LoadNextChunk()
    {
        if (_startingDelayFrames > 0)
        {
            _startingDelayFrames--;
            return;
        }

        _loader.LoadNextChunkOfItems();
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
    }

    public override void OnCartridgeStarted()
    {
    }

    public override bool ShouldLoadNextCartridge()
    {
        return _loader.IsFullyDone();
    }

    public override void Unload()
    {
    }
}
