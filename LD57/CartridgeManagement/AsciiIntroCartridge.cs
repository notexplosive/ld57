using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD57.CartridgeManagement;

public class AsciiIntroCartridge : Cartridge
{
    private readonly AsciiScreen _screen;
    private GridPosition _starPosition;
    private float _elapsedTime;

    public AsciiIntroCartridge(IRuntime runtime) : base(runtime)
    {
        _screen = Constants.CreateGameScreen();
        Tween.Add(new WaitSecondsTween(1f));

        _starPosition = new GridPosition();
    }

    public SequenceTween Tween { get; } = new();
    
    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.PointClamp);
    public override void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
        _screen.Draw(painter, new Vector2(0, _screen.TileSize / 4f));
    }

    public override void Update(float dt)
    {
        _elapsedTime += dt;

        var radius = 10;
        var position = new GridPosition((int)(MathF.Cos(_elapsedTime) * radius), (int)(MathF.Sin(_elapsedTime) * radius));
        
        _screen.PutTile(position, TileState.StringCharacter("X"));
        
        Tween.Update(dt);
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.IsAnyKeyDown())
        {
            Tween.SkipToEnd();
        }
    }

    public override void OnCartridgeStarted()
    {
        
    }

    public override bool ShouldLoadNextCartridge()
    {
        return Tween.IsDone();
    }

    public override void Unload()
    {
        
    }
}
