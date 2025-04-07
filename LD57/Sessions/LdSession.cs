using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.CartridgeManagement;
using LD57.Gameplay;
using LD57.Rendering;
using LD57.Rules;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace LD57.Sessions;

public class LdSession : Session
{
    private readonly SequenceTween _cutsceneTween = new();
    private readonly DialogueBox _dialogueBox;
    private readonly Inventory _inventory = new();
    private readonly Queue<ModalEvent> _modalQueue = new();
    private readonly PromptBox _promptBox;
    private readonly AsciiScreen _screen;
    private readonly TitleCard _titleCard;
    private readonly SequenceTween _transitionTween = new();
    private string? _currentZoneName;
    private float _inputTimer;
    private ActionButton _pendingActionButton;
    private Direction _pendingDirection = Direction.None;
    private Entity _player = new(new GridPosition(), new Invisible());
    private bool _skipClear;
    private bool _skipDrawingMain;
    private World _world = new(Constants.GameRoomSize, new WorldTemplate());
    private ITransition? _currentTransition;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = Constants.CreateGameScreen();
        _titleCard = new TitleCard(Constants.GameRoomSize);
        _dialogueBox = new DialogueBox(Constants.GameRoomSize);
        _promptBox = new PromptBox(Constants.GameRoomSize);
        LoadWorld(new WorldTemplate());
        OpenMainMenu();
    }

    private void OpenMainMenu()
    {
        // _skipDrawingMain = true;
        _currentTransition = new LsdTransition(_screen);

        var mainMenuPrompt = new Prompt(Constants.Title, Orientation.Vertical, [
            new PromptOption("Play", () => { FadeOutTransition(); }),
            new PromptOption("Credits", () =>
            {
                DisplayScriptedDialogueMessage("credits");
                OpenMainMenu();
            }),
            new PromptOption("Level Editor", () => { RequestLevelEditor?.Invoke(); })
        ]);

        DisplayPrompt(mainMenuPrompt);
    }

    private void CrossFadeTransition(ITransition lsdTransition, Action action)
    {
        _currentTransition = lsdTransition;
        
        _transitionTween.SkipToEnd();

        _transitionTween
            .Add(_currentTransition.FadeIn())
            .Add(new CallbackTween(action))
            ;
        
        FadeOutTransition();
    } 
    
    private void FadeOutTransition()
    {
        if (_currentTransition == null)
        {
            return;
        }
        
        _transitionTween
            .Add(_currentTransition.FadeOut())
            .Add(new CallbackTween(()=>_currentTransition = null))
            ;
    }

    public event Action? RequestLevelEditor;

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
            glyph.SetAnimation(Animations.FloatOnWater());
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
        if (_inventory.IsPlayingItemAnimation || !_transitionTween.IsDone())
        {
            return;
        }

        var frameInput = new FrameInput();

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

        _transitionTween.Update(dt);
        if (_transitionTween.IsDone())
        {
            _transitionTween.Clear();
        }

        _cutsceneTween.Update(dt);
        if (_cutsceneTween.IsDone())
        {
            _cutsceneTween.Clear();
        }

        if (!_skipClear)
        {
            _screen.Clear(TileState.Empty);
        }

        if (!_skipDrawingMain)
        {
            _world.PaintToScreen(_screen, dt);
            _inventory.PaintWorldOverlay(ActionButton.Primary, _screen, _world, _player, dt);
            _inventory.PaintWorldOverlay(ActionButton.Secondary, _screen, _world, _player, dt);

            _titleCard.PaintToScreen(_screen);

            // UI

            _inventory.DrawHud(_screen, _currentZoneName, dt);
        }

        _currentTransition?.PaintToScreen(dt);

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
            if (_dialogueBox.IsVisible)
            {
                if (_pendingActionButton != ActionButton.None)
                {
                    _dialogueBox.NextPage(_cutsceneTween);
                }
            }
            else if (_promptBox.IsVisible)
            {
                _promptBox.DoInput(_pendingDirection, _pendingActionButton);
            }
            else if (_modalQueue.Count > 0)
            {
                // if we have modals pending, don't react to any input
            }
            else
            {
                if (_pendingActionButton != ActionButton.None)
                {
                    _inventory.AnimateUse(_pendingActionButton, _world, _player, _cutsceneTween);
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
        _world.RequestEquipItem += EquipItem;

        foreach (var entity in _world.CurrentRoom.AllActiveEntities())
        {
            entity.SelfTriggerBehavior(BehaviorTrigger.OnEnter);
        }
    }

    private void EquipItem(ActionButton slot, Entity item)
    {
        _world.Destroy(item);
        if (_inventory.HasSomethingInSlot(slot))
        {
            var existingItem = _inventory.RemoveFromSlot(slot, _world, _player);
            DisplayDynamicDialogueMessage($"You dropped\n{Inventory.GetItemName(existingItem)}");
        }

        _inventory.Equip(slot, item);
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
        if (_currentTransition != null)
        {
            _transitionTween.Add(new CallbackTween(() =>
            {
                DisplayZoneNameImmediate(newZoneName);
            }));
        }
        else
        {
            DisplayZoneNameImmediate(newZoneName);
        }
    }

    private void DisplayZoneNameImmediate(string newZoneName)
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
            CrossFadeTransition(new WipeTransition(_screen, TileState.Empty), () =>
            {
                LoadWorld(worldTemplate);
            });

        }
    }
}
