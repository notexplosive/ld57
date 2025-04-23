using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD57.CartridgeManagement;

public class AsciiLoadingCartridge : Cartridge
{
    private readonly Loader _loader;
    private bool _doneLoading;
    private readonly AsciiScreen _screen;
    private int _startingDelayFrames = 5;
    private List<string> _statuses = new();
    private readonly int _heightLimit;

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

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.PointClamp);
    public override void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
        _screen.Draw(painter, new Vector2(0, _screen.TileSize / 4f));
    }

    public override void Update(float dt)
    {
        _screen.Clear(TileState.Empty);

        var lineNumber = 0;
        foreach (var status in _statuses)
        {
            _screen.PutString(new GridPosition(0,lineNumber), status);
            lineNumber++;
        }
        
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
        return _loader.IsDone();
    }

    public override void Unload()
    {
        
    }
}
