using ExplogineMonoGame;
using ExplogineMonoGame.Data;
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
    private float _turnTimer;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = new AsciiScreen(40, 22, 48);
        var finalRoomSize = _screen.RoomSize - new GridPosition(0, 3);
        _world = new World(finalRoomSize);
        
        _player = _world.AddEntity(new Entity(new GridPosition(5,5), new EntityAppearance(ResourceAlias.Entities, 0, Color.Orange)));
        _world.AddEntity(new Entity(new GridPosition(10,5), new EntityAppearance(ResourceAlias.Entities, 1, Color.White)));
        _world.AddEntity(new Entity(new GridPosition(62,6), new EntityAppearance(ResourceAlias.Entities, 1, Color.White)));
        
        _world.AddEntity(new Entity(new GridPosition(-5,-5), new EntityAppearance(ResourceAlias.Entities, 1, Color.White)));
        
        _world.CurrentRoom.CalculateLiveEntities();

        _world.Rules.AddRule(new CameraFollowsEntity(_player));
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        _inputDirection = Direction.None;
        
        if (input.Keyboard.GetButton(Keys.Left).IsDown)
        {
            _inputDirection = Direction.Left;
        }
        
        if (input.Keyboard.GetButton(Keys.Right).IsDown)
        {
            _inputDirection = Direction.Right;
        }
        
        if (input.Keyboard.GetButton(Keys.Up).IsDown)
        {
            _inputDirection = Direction.Up;
        }
        
        if (input.Keyboard.GetButton(Keys.Down).IsDown)
        {
            _inputDirection = Direction.Down;
        }
    }

    public override void Update(float dt)
    {
        _screen.Clear(TileState.Empty);

        _turnTimer += dt;
        
        if (_turnTimer > 0.1f)
        {
            if (_inputDirection != Direction.None)
            {
                _world.Rules.AttemptMoveInDirection(_player, _inputDirection);
            }

            _turnTimer = 0;
        }
        
        foreach (var entity in _world.CurrentRoom.AllEntities())
        {
            var renderedPosition = entity.Position - _world.CurrentRoom.TopLeftPosition;
            _screen.SetTile(renderedPosition, entity.TileState);
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
        _screen.Draw(painter);
    }
}
