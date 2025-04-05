using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.CartridgeManagement;
using LD57.Gameplay;
using LD57.Rendering;
using LD57.Rules;
using Microsoft.Xna.Framework;

namespace LD57.Sessions;

public class LdSession : Session
{
    private readonly Entity _player;
    private readonly AsciiScreen _screen;
    private readonly World _world;
    private Direction _inputDirection = Direction.None;
    private float _inputTimer;
    private readonly TweenableRectangle _rectangle = new(Rectangle.Empty);

    private readonly MultiplexTween _tween = new();

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = new AsciiScreen(40, 22, 48);
        var finalRoomSize = _screen.RoomSize - new GridPosition(0, 3);
        _world = new World(finalRoomSize);

        _player = _world.AddEntity(new Entity(new GridPosition(5, 5),
            LdResourceAssets.Instance.EntityTemplates["player"]));

        _world.AddEntity(new Entity(new GridPosition(10, 5), LdResourceAssets.Instance.EntityTemplates["crate"]));
        _world.AddEntity(new Entity(new GridPosition(12, 6), LdResourceAssets.Instance.EntityTemplates["crate"]));
        _world.AddEntity(new Entity(new GridPosition(15, 6), LdResourceAssets.Instance.EntityTemplates["button"]));
        _world.AddEntity(new Entity(new GridPosition(62, 6), LdResourceAssets.Instance.EntityTemplates["crate"]));
        _world.AddEntity(new Entity(new GridPosition(-5, -5), LdResourceAssets.Instance.EntityTemplates["crate"]));
        _world.AddEntity(new Entity(new GridPosition(15, 5), LdResourceAssets.Instance.EntityTemplates["water"]));
        _world.AddEntity(new Entity(new GridPosition(16, 5), LdResourceAssets.Instance.EntityTemplates["water"]));

        _world.Rules.AddRule(new CameraFollowsEntity(_player));

        _world.MoveCompleted += OnMoveCompleted;
    }

    private void OnMoveCompleted(MoveData data, MoveStatus status)
    {
        var entitiesAtDestination = _world.GetActiveEntitiesAt(data.Destination).ToList();
        var glyph = data.Mover.TweenableGlyph;
        if (status.WasInterrupted)
        {
            glyph.SetAnimation(Animations.MakeInPlaceNudge(data.Direction, _screen.TileSize / 4));
            return;
        }

        if (data.Mover.HasTag("Pushable"))
        {
            glyph.SetAnimation(Animations.MakeMoveNudge(data.Direction, _screen.TileSize / 4));
        }
        
        var waterAtDestination = _world.FilterToEntitiesWithTag(entitiesAtDestination, "Water").ToList();
        if (waterAtDestination.Count > 0 && data.Mover.HasTag("FloatsInWater"))
        {
            glyph.SetAnimation(Animations.FloatOnWater(waterAtDestination.First().TileState.ForegroundColor));
        }

        var buttonsAtDestination = _world.FilterToEntitiesWithTag(entitiesAtDestination, "Button").ToList();
        if (data.Mover.HasTag("PressesButtons") && buttonsAtDestination.Count > 0)
        {
            glyph.AddAnimation(Animations.PulseColorLoop(data.Mover.TileState.ForegroundColor, ResourceAlias.Color("button")));
        }
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _inputDirection = Direction.None;

        var frameInput = new InputState();

        if (frameInput.AnyDirectionTapped(input))
        {
            _inputTimer = 0;
        }

        _inputDirection = frameInput.HeldDirection(input);
    }

    public override void Update(float dt)
    {
        var allDrawnEntities = _world.CurrentRoom.AllVisibleEntitiesInDrawOrder();
        var allActiveEntities = _world.CurrentRoom.AllActiveEntities().ToList();
        foreach (var entity in allDrawnEntities)
        {
            entity.TweenableGlyph.RootTween.Update(dt);
        }

        _tween.Update(dt);

        if (_tween.IsDone())
        {
            _tween.Clear();
        }

        _screen.Clear(TileState.Empty);

        _inputTimer -= dt;

        if (_inputDirection == Direction.None)
        {
            _inputTimer = 0;
        }

        if (_inputTimer <= 0f)
        {
            if (_inputDirection != Direction.None)
            {
                var move = _world.Rules.AttemptMoveInDirection(_player, _inputDirection);
                if (!move.WasInterrupted)
                {
                    _player.TweenableGlyph.SetAnimation(Animations.MakeWalk(_inputDirection, _screen.TileSize / 4f));
                }
            }

            _inputTimer = 0.125f;
        }

        foreach (var entity in allDrawnEntities)
        {
            var renderedPosition = entity.Position - _world.CurrentRoom.TopLeftPosition;
            _screen.SetTile(renderedPosition, entity.TileState, entity.TweenableGlyph);
        }

        // UI

        if (_rectangle.Value.Width * _rectangle.Value.Height > 0)
        {
            _screen.PutRectangle(ResourceAlias.PopupFrame, new GridPosition(_rectangle.Value.Location),
                new GridPosition(_rectangle.Value.Location + _rectangle.Value.Size));
        }

        var bottomHudTopLeft = new GridPosition(0, 19);
        _screen.PutRectangle(ResourceAlias.PopupFrame, bottomHudTopLeft,
            bottomHudTopLeft + new GridPosition(_screen.Width - 1, 2));
        _screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), "Status: OK");
        _screen.PutString(bottomHudTopLeft + new GridPosition(2, 0), "Z[ ]");
        _screen.SetTile(bottomHudTopLeft + new GridPosition(4, 0), TileState.Sprite(ResourceAlias.Entities, 14));
        _screen.PutString(bottomHudTopLeft + new GridPosition(7, 0), "X[ ]", Color.Gray);
        _screen.SetTile(bottomHudTopLeft + new GridPosition(9, 0), TileState.Sprite(ResourceAlias.Entities, 15));
        
        // Cleanup
        _world.UpdateEntityList();
    }

    public override void Draw(Painter painter)
    {
        painter.Clear(ColorExtensions.FromRgbHex(0x021c04));
        _screen.Draw(painter);
    }
}
