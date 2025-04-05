using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.CartridgeManagement;
using LD57.Gameplay;
using LD57.Rendering;
using LD57.Rules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Sessions;

public class LdSession : Session
{
    private readonly AsciiScreen _screen;
    private readonly World _world;
    private readonly Entity _player;
    private Direction _inputDirection = Direction.None;
    private float _inputTimer;

    private MultiplexTween _tween = new();
    private TweenableRectangle _rectangle = new(Rectangle.Empty);

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = new AsciiScreen(40, 22, 48);
        var finalRoomSize = _screen.RoomSize - new GridPosition(0, 3);
        _world = new World(finalRoomSize);
        
        _player = _world.AddEntity(new Entity(new GridPosition(5,5), LdResourceAssets.Instance.EntityTemplates["player"]));
        
        _world.AddEntity(new Entity(new GridPosition(10,5), LdResourceAssets.Instance.EntityTemplates["crate"]));
        _world.AddEntity(new Entity(new GridPosition(12,6), LdResourceAssets.Instance.EntityTemplates["crate"]));
        _world.AddEntity(new Entity(new GridPosition(62,6), LdResourceAssets.Instance.EntityTemplates["crate"]));
        _world.AddEntity(new Entity(new GridPosition(-5,-5), LdResourceAssets.Instance.EntityTemplates["crate"]));
        
        _world.Rules.AddRule(new CameraFollowsEntity(_player));
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
                _world.Rules.AttemptMoveInDirection(_player, _inputDirection);
            }

            _inputTimer = 0.125f;
        }
        
        foreach (var entity in _world.CurrentRoom.AllEntities())
        {
            var renderedPosition = entity.Position - _world.CurrentRoom.TopLeftPosition;
            _screen.SetTile(renderedPosition, entity.TileState);
        }
        
        // UI

        if (_rectangle.Value.Width * _rectangle.Value.Height > 0)
        {
            _screen.PutRectangle(ResourceAlias.PopupFrame, new GridPosition(_rectangle.Value.Location), new GridPosition(_rectangle.Value.Location + _rectangle.Value.Size));
        }
        
        var bottomHudTopLeft = new GridPosition(0, 19);
        _screen.PutRectangle(ResourceAlias.PopupFrame, bottomHudTopLeft,
            bottomHudTopLeft + new GridPosition(_screen.Width - 1, 2));
        _screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), "Status: OK");
        _screen.PutString(bottomHudTopLeft + new GridPosition(2, 0), "Z[ ]");
        _screen.SetTile(bottomHudTopLeft + new GridPosition(4, 0), TileState.Sprite(ResourceAlias.Entities, 14));
        _screen.PutString(bottomHudTopLeft + new GridPosition(7, 0), "X[ ]", Color.Gray);
        _screen.SetTile(bottomHudTopLeft + new GridPosition(9, 0), TileState.Sprite(ResourceAlias.Entities, 15));
    }

    public override void Draw(Painter painter)
    {
        painter.Clear(ColorExtensions.FromRgbHex(0x021c04));
        _screen.Draw(painter);
    }
}
