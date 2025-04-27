using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using LD57.CartridgeManagement;
using LD57.Gameplay.Behaviors;
using LD57.Gameplay.Triggers;
using LD57.Rendering;

namespace LD57.Gameplay;

public class World
{
    private readonly List<Entity> _entities = new();
    private readonly HashSet<Entity> _entitiesToRemove = new();
    private readonly GridPosition _roomSize;

    public World(GridPosition roomSize, WorldTemplate worldTemplate, bool isEditMode = false)
    {
        IsEditMode = isEditMode;
        _roomSize = roomSize;
        Rules = new RuleComputer(this);
        CurrentRoom = GetRoomAt(new GridPosition(0, 0));

        PopulateFromTemplate(worldTemplate);
    }

    public bool IsEditMode { get; }
    public Room CurrentRoom { get; private set; }
    public RuleComputer Rules { get; }

    public GridPosition CameraPosition { get; set; }

    public void PopulateFromTemplate(WorldTemplate worldTemplate)
    {
        foreach (var placedEntity in worldTemplate.PlacedEntities)
        {
            if (placedEntity.TemplateName == "player")
            {
                continue;
            }

            if (LdResourceAssets.Instance.EntityTemplates.ContainsKey(placedEntity.TemplateName))
            {
                AddEntityFast(CreateEntityFromPlacement(placedEntity));
            }
            else
            {
                // no template, this is a command-only entity
                AddEntityFast(CreateTriggerEntityFromPlacement(placedEntity));
            }
        }

        CurrentRoom.RecalculateLiveEntities();
    }

    private Entity CreateEntityFromPlacement(PlacedEntity placedEntity)
    {
        var entityTemplate = ResourceAlias.EntityTemplate(placedEntity.TemplateName);
        return CreateEntityFromTemplate(entityTemplate, placedEntity.Position, placedEntity.ExtraState);
    }

    public Entity CreateEntityFromTemplate(EntityTemplate entityTemplate, GridPosition position,
        Dictionary<string, string> extraState)
    {
        var entity = new Entity(this, position, entityTemplate);
        entity.State.AddFromDictionary(extraState);
        SetupNormalEntityBehaviors(entity);
        return entity;
    }

    private void SetupNormalEntityBehaviors(Entity entity)
    {
        if (entity.HasTag("Water"))
        {
            if (!IsEditMode)
            {
                entity.TweenableGlyph.SetAnimation(Animations.WaterSway(Client.Random.Dirty.NextFloat() * 100));
            }

            entity.AddBehavior(new Water());
        }

        // if (entity.HasTag("Crystal"))
        // {
        //     if (!IsEditMode)
        //     {
        //         entity.TweenableGlyph.SetAnimation(Animations.CrystalShimmer(ResourceAlias.Color("blood"),
        //             ResourceAlias.Color("white"), ResourceAlias.Color("blue_text")));
        //     }
        // }

        // if (entity.HasTag("Crystal"))
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnTouch, payload =>
        //     {
        //         if (payload.Entity != null && payload.Entity.HasTag("Player"))
        //         {
        //             RequestClaimCrystal?.Invoke(entity);
        //         }
        //     });
        // }

        // if (entity.HasTag("Item"))
        // {
        //     var humanReadableName = Inventory.GetHumanReadableName(entity);
        //
        //     entity.AddBehavior(BehaviorTrigger.OnTouch, payload =>
        //     {
        //         if (payload.Entity != null && payload.Entity.HasTag("Player"))
        //         {
        //             // RequestShowDynamicMessage?.Invoke($"You find \n{humanReadableName}");
        //             RequestShowPrompt?.Invoke(new Prompt($"Equip {humanReadableName}?", Orientation.Vertical, [
        //                 new PromptOption("Equip to [Z]",
        //                     () => { RequestEquipItem?.Invoke(ActionButton.Primary, entity); }),
        //                 new PromptOption("Equip to [X]",
        //                     () => { RequestEquipItem?.Invoke(ActionButton.Secondary, entity); }),
        //                 new PromptOption("Leave it", () => { })
        //             ]));
        //         }
        //     });
        // }

        if (entity.HasTag("TransformWhenSteppedOff"))
        {
            entity.AddBehavior(new TransformWhenSteppedOff());
        }

        if (entity.HasTag("Signal"))
        {
            entity.AddBehavior(new SignalColor());

            if (entity.HasTag("Button"))
            {
                entity.AddBehavior(new Button());
            }

            if (entity.HasTag("Bridge"))
            {
                entity.AddBehavior(new SetStateBasedOnSignal("is_surfaced", "is_inverted"));
                entity.AddBehavior(new SwapOutEntityWhenState("is_surfaced", false, "water"));
            }

            if (entity.HasTag("Door"))
            {
                entity.AddBehavior(new SetStateBasedOnSignal("is_open", "is_inverted"));
                entity.AddBehavior(new ChangeTileFrameBasedOnState("is_open", "open_frame", "closed_frame"));
                entity.AddBehavior(new AddTagsWhenState("is_open", false, ["Solid"]));
            }
        }
    }

    public IEnumerable<Entity> EntitiesInSameRoom(GridPosition position)
    {
        return CalculateEntitiesInRoom(GetRoomCornersAt(position), true);
    }

    public IEnumerable<Entity> CalculateEntitiesInRoom(GridPositionCorners corners, bool onlyActive)
    {
        var rectangle = corners.Rectangle(true);
        foreach (var entity in onlyActive ? AllActiveEntities() : AllEntitiesIncludingInactive())
        {
            if (rectangle.Contains(entity.Position.ToPoint()))
            {
                yield return entity;
            }
        }
    }

    public event Action? AttemptedVictory;
    public event Action? EnteredSanctuary;
    public event Action<string>? RequestLoad;
    public event Action<string>? RequestZoneNameChange;
    public event Action<string>? RequestShowScriptedMessage;
    public event Action<Entity>? RequestClaimCrystal;
    public event Action<Prompt>? RequestShowPrompt;
    public event Action<AmbientPlayMode, string, SoundEffectSettings>? RequestAmbientSound;
    public event Action<string, GridPosition>? RequestSpawnFromStorage;
    public event Action<ActionButton, Entity>? RequestEquipItem;

    private Entity CreateTriggerEntityFromPlacement(PlacedEntity placedEntity)
    {
        var entity = new Entity(this, placedEntity.Position, new Invisible());
        entity.State.AddFromDictionary(placedEntity.ExtraState);
        SetupTriggerEntityBehaviors(entity);
        return entity;
    }

    public void SetupTriggerEntityBehaviors(Entity entity)
    {
        // var parsedCommand = new ParsedCommand(entity.State.GetString(Constants.CommandKey));

        // if (parsedCommand.CommandName == "preserve")
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnEnter, () => { EnteredSanctuary?.Invoke(); });
        // }
        //
        // if (parsedCommand.CommandName == "vault")
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnTouch, payload =>
        //     {
        //         if (payload.Entity?.HasTag("Player") == true)
        //         {
        //             AttemptedVictory?.Invoke();
        //         }
        //     });
        // }
        //
        // if (parsedCommand.CommandName == "load")
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnTouch, payload =>
        //     {
        //         if (payload.Entity?.HasTag("Player") == true)
        //         {
        //             RequestLoad?.Invoke(parsedCommand.ArgAsString(0));
        //         }
        //     });
        // }
        //
        // if (parsedCommand.CommandName == "zone")
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnEnter, () => { RequestZoneNameChange?.Invoke(parsedCommand.AllArgsJoined(" ")); });
        // }
        //
        // if (parsedCommand.CommandName == "show")
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnTouch, payload =>
        //     {
        //         if (payload.Entity?.HasTag("Player") == true)
        //         {
        //             RequestShowScriptedMessage?.Invoke(parsedCommand.ArgAsString(0));
        //         }
        //     });
        // }
        //
        // if (parsedCommand.CommandName == "store")
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnReset,
        //         () => { RequestSpawnFromStorage?.Invoke(parsedCommand.ArgAsString(0), entity.Position); });
        // }
        //
        // if (parsedCommand.CommandName == "ambient")
        // {
        //     entity.AddBehavior(BehaviorTrigger.OnEnter,
        //         () =>
        //         {
        //             RequestAmbientSound?.Invoke(AmbientPlayMode.Play, parsedCommand.ArgAsString(0), new SoundEffectSettings {Volume = parsedCommand.ArgAsFloat(1, 1)});
        //         });
        //     entity.AddBehavior(BehaviorTrigger.OnExit,
        //         () => { RequestAmbientSound?.Invoke(AmbientPlayMode.Stop, parsedCommand.ArgAsString(0), new SoundEffectSettings()); });
        // }
    }

    public void SetCurrentRoom(Room newRoom)
    {
        var previousRoom = CurrentRoom;
        CurrentRoom = newRoom;
        CameraPosition = newRoom.TopLeft;

        if (previousRoom.TopLeft != newRoom.TopLeft)
        {
            foreach (var entity in previousRoom.AllActiveEntities())
            {
                entity.TriggerBehavior(ExitTrigger.Instance);
            }

            foreach (var entity in newRoom.AllActiveEntities())
            {
                entity.TriggerBehavior(EnterTrigger.Instance);
            }
        }
    }

    public IEnumerable<Entity> AllEntitiesIncludingInactive()
    {
        return _entities;
    }

    public IEnumerable<Entity> AllActiveEntitiesCached()
    {
        return AllActiveEntities().ToList();
    }

    public IEnumerable<Entity> AllActiveEntities()
    {
        foreach (var entity in _entities)
        {
            if (entity.IsActive)
            {
                yield return entity;
            }
        }
    }

    private void AddEntityFast(Entity entity)
    {
        if (entity.World != this)
        {
            throw new Exception("Entity is not from this world, panic!");
        }

        _entities.Add(entity);
        entity.Start(this);
    }

    public Entity AddEntity(Entity entity)
    {
        AddEntityFast(entity);
        if (CurrentRoom.Contains(entity.Position))
        {
            CurrentRoom.RecalculateLiveEntities();
        }

        return entity;
    }

    public Room GetRoomAt(GridPosition position)
    {
        var corners = GetRoomCornersAt(position);
        return new Room(this, corners.TopLeft, corners.BottomRight);
    }

    private GridPositionCorners GetRoomCornersAt(GridPosition position)
    {
        var inflatedRoomSize = _roomSize + new GridPosition(1, 1);
        var x = position.X % inflatedRoomSize.X;
        if (x < 0)
        {
            x += inflatedRoomSize.X;
        }

        var y = position.Y % inflatedRoomSize.Y;
        if (y < 0)
        {
            y += inflatedRoomSize.Y;
        }

        var topLeft = position - new GridPosition(x, y);
        return new GridPositionCorners(topLeft, topLeft + _roomSize);
    }

    public IEnumerable<Entity> GetActiveEntitiesAt(GridPosition position)
    {
        foreach (var entity in CurrentRoom.Contains(position) ? CurrentRoom.AllActiveEntities() : AllActiveEntities())
        {
            if (entity.Position == position)
            {
                yield return entity;
            }
        }
    }

    public IEnumerable<Entity> FilterToEntitiesWithTag(List<Entity> entities, string tag)
    {
        foreach (var entity in entities)
        {
            if (entity.HasTag(tag))
            {
                yield return entity;
            }
        }
    }

    public event Action<MoveData, MoveStatus>? MoveCompleted;

    public void OnMoveCompleted(MoveData moveData, MoveStatus status)
    {
        var entities = new List<Entity>();
        var roomCornersAtSource = GetRoomCornersAt(moveData.Source);
        var roomCornersAtDestination = GetRoomCornersAt(moveData.Destination);

        entities.AddRange(CalculateEntitiesInRoom(roomCornersAtSource, true));

        if (roomCornersAtSource != roomCornersAtDestination && status.WasSuccessful)
        {
            // If the move spanned multiple rooms AND the move was successful, update both rooms
            entities.AddRange(CalculateEntitiesInRoom(roomCornersAtDestination, true));
        }

        foreach (var entity in entities)
        {
            if (entity.Position == moveData.Destination)
            {
                // even if the move failed, we still count that as a "touch"
                entity.TriggerBehavior(new TouchTrigger(moveData.Mover));
            }

            if (status.WasSuccessful && entity.Position == moveData.Source)
            {
                entity.TriggerBehavior(new SteppedOffTrigger(moveData.Mover));
            }

            entity.TriggerBehavior(new EntityMovedTrigger(moveData.Mover));
        }

        MoveCompleted?.Invoke(moveData, status);
    }

    public void Destroy(Entity entity)
    {
        _entitiesToRemove.Add(entity);
    }

    public void UpdateEntityList()
    {
        foreach (var entity in _entitiesToRemove)
        {
            _entities.Remove(entity);
        }

        _entitiesToRemove.Clear();

        CurrentRoom.RecalculateLiveEntities();
    }

    public void PaintToScreen(AsciiScreen screen, float dt)
    {
        var allDrawnEntities = CurrentRoom.AllVisibleEntitiesInDrawOrder();

        foreach (var entity in allDrawnEntities)
        {
            entity.TweenableGlyph.RootTween.Update(dt);
        }

        foreach (var entity in allDrawnEntities)
        {
            var renderedPosition = entity.Position - CameraPosition;
            if (entity.TileState.HasValue)
            {
                screen.PutTile(renderedPosition, entity.TileState.Value, entity.TweenableGlyph);
            }
        }
    }

    public bool IsDestroyed(Entity water)
    {
        return _entitiesToRemove.Contains(water) || !_entities.Contains(water);
    }

    public void ClearAllEntities()
    {
        _entities.Clear();
        _entitiesToRemove.Clear();
    }
}
