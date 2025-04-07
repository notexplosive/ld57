using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Gameplay;
using LD57.Rendering;
using LD57.Rules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace LD57.Sessions;

public class LdSession : Session
{
    private readonly AsciiScreen _screen;
    private Direction _pendingDirection = Direction.None;
    private float _inputTimer;
    private Entity _player = new(new GridPosition(), new Invisible());
    private World _world = new(Constants.GameRoomSize, new WorldTemplate());
    private string? _currentZoneName;
    private readonly SequenceTween _cutsceneTween = new();
    private readonly TitleCard _titleCard;
    private readonly DialogueBox _dialogueBox;
    private readonly PromptBox _promptBox;
    private ActionButton _pendingActionButton;
    private Queue<ModalEvent> _modalQueue = new();

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = Constants.CreateGameScreen();
        _titleCard = new TitleCard(Constants.GameRoomSize);
        _dialogueBox = new DialogueBox(Constants.GameRoomSize);
        _promptBox = new PromptBox(Constants.GameRoomSize);
        LoadWorld(new WorldTemplate());
    }

    private void OnMoveCompleted(MoveData data, MoveStatus status)
    {
        var entitiesAtDestination = _world.GetActiveEntitiesAt(data.Destination).ToList();
        var glyph = data.Mover.TweenableGlyph;
        if (!status.WasSuccessful)
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
            glyph.SetAnimation(Animations.FloatOnWater(waterAtDestination.First().TileState!.Value.ForegroundColor));
        }

        var buttonsAtDestination = _world.FilterToEntitiesWithTag(entitiesAtDestination, "Button").ToList();
        if (data.Mover.HasTag("PressesButtons") && buttonsAtDestination.Count > 0)
        {
            glyph.AddAnimation(Animations.PulseColorLoop(data.Mover.TileState!.Value.ForegroundColor,
                ResourceAlias.Color("button")));
        }
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var frameInput = new FrameInput();
        
        if (_dialogueBox.IsVisible)
        {
            if (FrameInput.AnyActionTapped(input))
            {
                _dialogueBox.NextPage(_cutsceneTween);
            }

            return;
        }
        
        if (FrameInput.PrimaryActionTapped(input))
        {
            _pendingActionButton = ActionButton.Primary;
        }

        if (FrameInput.SecondaryActionTapped(input))
        {
            _pendingActionButton = ActionButton.Secondary;
        }
        
        if (frameInput.AnyDirectionTapped(input))
        {
            _inputTimer = 0;
        }

        if (_pendingDirection == Direction.None)
        {
            _pendingDirection = frameInput.HeldDirection(input);
        }
    }

    public override void Update(float dt)
    {
        TickInputTimer(dt);

        UpdateModalEvents();

        _screen.Clear(TileState.Empty);
        _world.PaintToScreen(_screen, dt);
        
        _cutsceneTween.Update(dt);
        if (_cutsceneTween.IsDone())
        {
            _cutsceneTween.Clear();
        }
        
        _titleCard.PaintToScreen(_screen);

        // UI
        var bottomHudTopLeft = new GridPosition(0, 19);
        _screen.PutFrameRectangle(ResourceAlias.PopupFrame, bottomHudTopLeft,
            bottomHudTopLeft + new GridPosition(_screen.Width - 1, 2));
        _screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), "Status: OK");
        _screen.PutString(bottomHudTopLeft + new GridPosition(2, 0), "Z[ ]");
        _screen.PutTile(bottomHudTopLeft + new GridPosition(4, 0), TileState.Sprite(ResourceAlias.Entities, 14));
        _screen.PutString(bottomHudTopLeft + new GridPosition(7, 0), "X[ ]", Color.Gray);
        _screen.PutTile(bottomHudTopLeft + new GridPosition(9, 0), TileState.Sprite(ResourceAlias.Entities, 15));

        // Dialogue
        if (_dialogueBox.IsVisible)
        {
            _dialogueBox.PaintToScreen(_screen);
        }

        if (_promptBox.IsVisible)
        {
            _promptBox.PaintToScreen(_screen);
            _promptBox.Update(dt);

            if (_promptBox.HasMadeAChoice && _cutsceneTween.IsDone())
            {
                _promptBox.DoCloseAnimation(_cutsceneTween);
            }
        }
        
        // Cleanup
        _world.UpdateEntityList();
    }

    private void UpdateModalEvents()
    {
        if (_modalQueue.Count > 0)
        {
            var topModalEvent = _modalQueue.Peek();
            if (topModalEvent.IsDone())
            {
                _modalQueue.Dequeue();
            }
        }

        // Check size again because we might have modified it
        if (_modalQueue.Count > 0)
        {
            var topModalEvent = _modalQueue.Peek();
            if (!topModalEvent.IsRunning)
            {
                topModalEvent.Execute();
            }
        }
    }

    private void TickInputTimer(float dt)
    {
        _inputTimer -= dt;

        if (_pendingDirection == Direction.None)
        {
            _inputTimer = 0;
        }

        if (_inputTimer <= 0f)
        {
            if (_promptBox.IsVisible)
            {
                _promptBox.DoInput(_pendingDirection, _pendingActionButton);
            }
            else
            {
                if (_pendingDirection != Direction.None)
                {
                    var move = _world.Rules.AttemptMoveInDirection(_player, _pendingDirection);
                    if (move.WasSuccessful)
                    {
                        _player.TweenableGlyph.SetAnimation(Animations.MakeWalk(_pendingDirection,
                            _screen.TileSize / 4f));
                    }
                }
            }

            _inputTimer = 0.125f;
        }

        _pendingActionButton = ActionButton.None;
        _pendingDirection = Direction.None;
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
            _currentZoneName = null;
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
        _world.RequestLoad += TransitionWorld;
        _world.RequestZoneNameChange += DisplayZoneName;
        _world.RequestShowScriptedMessage += DisplayScriptedDialogueMessage;
        _world.RequestShowDynamicMessage += DisplayDynamicDialogueMessage;
        _world.RequestShowPrompt += DisplayPrompt;
        
        foreach (var entity in _world.CurrentRoom.AllActiveEntities())
        {
            entity.SelfTriggerBehavior(BehaviorTrigger.OnEnter);
        }
    }

    private void DisplayPrompt(Prompt prompt)
    {
        _modalQueue.Enqueue(new PromptModalEvent(_promptBox, prompt, _cutsceneTween));
    }

    private void DisplayDynamicDialogueMessage(string rawMessage)
    {
        _modalQueue.Enqueue(new DialogueModalEvent(new MessageContent(rawMessage), _dialogueBox, _cutsceneTween));
    }
    
    private void DisplayScriptedDialogueMessage(string messageName)
    {
        _modalQueue.Enqueue(new DialogueModalEvent(ResourceAlias.Messages(messageName), _dialogueBox, _cutsceneTween));
    }

    private void DisplayZoneName(string newZoneName)
    {
        if (_currentZoneName != newZoneName)
        {
            _currentZoneName = newZoneName;
            _titleCard.DoAnimation(_cutsceneTween, newZoneName);
        }
    }

    private void TransitionWorld(string worldName)
    {
        var worldData = Client.Debug.RepoFileSystem.GetDirectory("Resource/Worlds").ReadFile(worldName + ".json");
        var worldTemplate = JsonConvert.DeserializeObject<WorldTemplate>(worldData);
        if (worldTemplate != null)
        {
            LoadWorld(worldTemplate);
        }
    }
}
