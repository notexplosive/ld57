﻿using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;
using LD57.Gameplay;
using LD57.Rendering;
using LD57.Rules;
using Microsoft.Xna.Framework;

namespace LD57.Sessions;

public class LdSession : Session
{
    private readonly AsciiScreen _screen;
    private Direction _inputDirection = Direction.None;
    private float _inputTimer;
    private Entity _player = new(new GridPosition(), new Invisible());
    private World _world = new(Constants.GameRoomSize, new WorldTemplate());

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = Constants.CreateGameScreen();
        LoadWorld(new WorldTemplate());
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
            glyph.AddAnimation(Animations.PulseColorLoop(data.Mover.TileState.ForegroundColor,
                ResourceAlias.Color("button")));
        }
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var frameInput = new InputState();

        if (frameInput.AnyDirectionTapped(input))
        {
            _inputTimer = 0;
        }

        if (_inputDirection == Direction.None)
        {
            _inputDirection = frameInput.HeldDirection(input);
        }
    }

    public override void Update(float dt)
    {
        var allActiveEntities = _world.CurrentRoom.AllActiveEntities().ToList();
        UpdateInputTimer(dt);

        _screen.Clear(TileState.Empty);
        _world.PopulateOnScreen(_screen, dt);

        // UI
        var bottomHudTopLeft = new GridPosition(0, 19);
        _screen.PutFrameRectangle(ResourceAlias.PopupFrame, bottomHudTopLeft,
            bottomHudTopLeft + new GridPosition(_screen.Width - 1, 2));
        _screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), "Status: OK");
        _screen.PutString(bottomHudTopLeft + new GridPosition(2, 0), "Z[ ]");
        _screen.PutTile(bottomHudTopLeft + new GridPosition(4, 0), TileState.Sprite(ResourceAlias.Entities, 14));
        _screen.PutString(bottomHudTopLeft + new GridPosition(7, 0), "X[ ]", Color.Gray);
        _screen.PutTile(bottomHudTopLeft + new GridPosition(9, 0), TileState.Sprite(ResourceAlias.Entities, 15));

        // Cleanup
        _world.UpdateEntityList();
    }

    private void UpdateInputTimer(float dt)
    {
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

        _inputDirection = Direction.None;
    }

    public override void Draw(Painter painter)
    {
        painter.Clear(ResourceAlias.Color("background"));
        _screen.Draw(painter, new Vector2(0, _screen.TileSize / 4f));
    }

    public void LoadWorld(WorldTemplate worldTemplate, GridPosition? playerSpawnPoint = null)
    {
        var playerSpawn = new GridPosition(0, 0);
        if (playerSpawnPoint.HasValue)
        {
            playerSpawn = playerSpawnPoint.Value;
        }
        else
        {
            var bakedInPlayerSpawn = worldTemplate.GetPlayerEntity();

            if (bakedInPlayerSpawn != null)
            {
                playerSpawn = bakedInPlayerSpawn.Position;
            }
        }

        _world = new World(Constants.GameRoomSize, worldTemplate);
        _player = _world.AddEntity(new Entity(playerSpawn, ResourceAlias.EntityTemplate("player")));

        _world.Rules.AddRule(new CameraFollowsEntity(_world, _player));
        _world.MoveCompleted += OnMoveCompleted;
    }
}
