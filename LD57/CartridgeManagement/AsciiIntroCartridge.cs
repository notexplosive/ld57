using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD57.CartridgeManagement;

public class AsciiIntroCartridge : Cartridge
{
    private readonly List<IntroGlyph> _glyphs = new();
    private readonly List<IntroParticle> _particles = new();
    private readonly AsciiScreen _screen;
    private float _elapsedTime;
    private bool _isDone;

    public AsciiIntroCartridge(IRuntime runtime) : base(runtime)
    {
        _screen = Constants.CreateGameScreen();

        var center = _screen.RoomSize / 2;
        var text = "notexplosive.net";
        var textStart = center - new GridPosition(text.Length / 2, 0);

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            var gridPosition = textStart + new GridPosition(index, 0);
            var introGlyph = new IntroGlyph(gridPosition, character, new TweenableGlyph());
            _glyphs.Add(introGlyph);
            introGlyph.TweenableGlyph.ForegroundColorOverride.Value = new Color(1, 1, 1, 0);
            introGlyph.TweenableGlyph.ShouldOverrideColor = true;
        }

        for (var index = 0; index < _glyphs.Count; index++)
        {
            var glyph = _glyphs[index];
            glyph.TweenableGlyph.RootTween.Add(new SequenceTween()
                    .Add(new WaitSecondsTween(index / 15f))
                    .Add(new CallbackTween(() => { ExplodeAt(glyph.Position); }))
                    .Add(glyph.TweenableGlyph.StartOverridingColor)
                    .Add(glyph.TweenableGlyph.Scale.CallbackSetTo(1.25f))
                    .Add(glyph.TweenableGlyph.ForegroundColorOverride.CallbackSetTo(ResourceAlias.Color("default")))
                    .Add(new MultiplexTween()
                        .Add(glyph.TweenableGlyph.Scale.TweenTo(0.8f, 0.05f, Ease.QuadSlowFast))
                        //.Add(glyph.TweenableGlyph.ForegroundColorOverride.TweenTo(ResourceAlias.Color("default"), 0.05f, Ease.Linear))
                    )
                    .Add(glyph.TweenableGlyph.Scale.TweenTo(1f, 0.05f, Ease.QuadFastSlow))
                )
                ;
        }
    }

    public static float Gravity => 150;
    public static float BurstStartingVerticalVelocity => 50;
    public static float BurstStartingVerticalVelocityBase => 10;
    public static float BurstSpreadVariance => 20;

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.PointClamp);

    private void ExplodeAt(GridPosition position)
    {
        for (int i = 0; i < 3; i++)
        {
            var introParticle = new IntroParticle(position.ToVector2() + Client.Random.Dirty.NextNormalVector2(),
                Client.Random.Dirty.GetRandomElement(["*", ".", "^", ",", "`",]));
            introParticle.Velocity =
                new Vector2(Client.Random.Dirty.NextFloat(-BurstSpreadVariance, BurstSpreadVariance),
                    -BurstStartingVerticalVelocity * Client.Random.Dirty.NextFloat() -
                    BurstStartingVerticalVelocityBase);
            _particles.Add(introParticle);
        }
    }

    public override void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
        _screen.Draw(painter, new Vector2(0, _screen.TileSize / 4f));
    }

    public override void Update(float dt)
    {
        _elapsedTime += dt;
        _screen.Clear(TileState.TransparentEmpty);

        foreach (var glyph in _glyphs)
        {
            glyph.TweenableGlyph.RootTween.Update(dt);
        }

        foreach (var particle in _particles)
        {
            _screen.PutTile(particle.RenderedPosition, particle.TileState);

            particle.Update(dt);
        }

        for (var index = 0; index < _glyphs.Count; index++)
        {
            var glyph = _glyphs[index];
            _screen.PutTile(glyph.Position, glyph.Tile, glyph.TweenableGlyph);
        }
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.IsAnyKeyDown())
        {
            _isDone = true;
        }
    }

    public override void OnCartridgeStarted()
    {
    }

    public override bool ShouldLoadNextCartridge()
    {
        return _isDone || _elapsedTime > 4f;
    }

    public override void Unload()
    {
    }

    private class IntroParticle
    {
        private readonly string _stringContent;

        public IntroParticle(Vector2 position, string stringContent)
        {
            TruePosition = position;
            _stringContent = stringContent;
        }

        public Vector2 TruePosition { get; private set; }
        public Vector2 Velocity { get; set; }

        public TileState TileState => TileState.StringCharacter(_stringContent, Color.Gray);

        public GridPosition RenderedPosition => TruePosition.RoundToGridPosition();

        public void Update(float dt)
        {
            TruePosition += Velocity * dt;
            Velocity += new Vector2(0, Gravity) * dt;
        }
    }

    private class IntroGlyph
    {
        public IntroGlyph(GridPosition position, char character, TweenableGlyph tweenableGlyph)
        {
            Position = position;
            Character = character.ToString();
            TweenableGlyph = tweenableGlyph;
        }

        public GridPosition Position { get; }
        public string Character { get; }
        public TweenableGlyph TweenableGlyph { get; }
        public TileState Tile => TileState.StringCharacter(Character);
    }
}
