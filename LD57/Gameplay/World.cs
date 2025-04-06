using System;
using System.Collections.Generic;
using System.Linq;
using LD57.CartridgeManagement;
using LD57.Rendering;

namespace LD57.Gameplay;

public class World
{
    private readonly List<Entity> _entities = new();
    private readonly HashSet<Entity> _entitiesToRemove = new();
    private readonly GridPosition _roomSize;

    public World(GridPosition roomSize, WorldTemplate worldTemplate)
    {
        _roomSize = roomSize;
        Rules = new RuleComputer(this);
        var gridPosition = new GridPosition(0, 0);
        CurrentRoom = new Room(this, gridPosition, gridPosition + roomSize);

        foreach (var placedEntity in worldTemplate.PlacedEntities)
        {
            if (placedEntity.TemplateName == "player")
            {
                continue;
            }

            if (LdResourceAssets.Instance.EntityTemplates.ContainsKey(placedEntity.TemplateName))
            {
                AddEntityFast(CreateEntityFromTemplate(placedEntity));
            }
            else
            {
                // no template, this is a command-only entity
                AddEntityFast(CreateTriggerEntityFromTemplate(placedEntity));
            }
        }

        CurrentRoom.RecalculateLiveEntities();
    }

    private Entity CreateEntityFromTemplate(PlacedEntity placedEntity)
    {
        var entityTemplate = ResourceAlias.EntityTemplate(placedEntity.TemplateName);
        var entity = new Entity(placedEntity.Position, entityTemplate);
        entity.State.AddFromDictionary(placedEntity.ExtraState);

        if (entity.HasTag("Signal"))
        {
            var channel = entity.State.GetInt("channel") ?? 0;

            entity.AddBehavior(new EntityBehavior(BehaviorTrigger.OnWorldStart, () =>
            {
                var signalColor = ResourceAlias.Color("signal_" + channel);
                if (entity.Appearance != null && entity.Appearance.TileState.HasValue)
                {
                    entity.Appearance.TileState = entity.Appearance.TileState.Value with { ForegroundColor = signalColor };
                }
            }));
            
            if (entity.HasTag("Button"))
            {
                entity.AddBehavior(new EntityBehavior(BehaviorTrigger.OnTouch, () =>
                {
                    var isPressed = CurrentRoom.AllActiveEntities()
                        .Where(a => a.Position == entity.Position).Any(a => a.HasTag("PressesButton"));

                    var wasPressed = entity.State.GetBool("is_pressed");
                    entity.State.Set("is_pressed", isPressed);

                    if (wasPressed != isPressed)
                    {
                        foreach (var otherEntities in CurrentRoom.AllActiveEntities()
                                     .Where(a => a.State.GetInt("channel") == channel))
                        {
                            otherEntities.TriggerBehavior(BehaviorTrigger.OnSignalChange);
                        }
                    }
                }));
            }

            if (entity.HasTag("Door"))
            {
                entity.AddBehavior(new EntityBehavior(BehaviorTrigger.OnSignalChange, () =>
                {
                    var allButtonsPressed = CurrentRoom.AllActiveEntities()
                        .Where(a => a.HasTag("Button") && a.State.GetInt("channel") == channel)
                        .All(a => a.State.GetBool("is_pressed") == true);

                    entity.State.Set("is_open", allButtonsPressed);
                }));
                
                entity.AddBehavior(new EntityBehavior(BehaviorTrigger.OnStateChanged, () =>
                {
                    // if state == is_open
                    // then change tile
                }));
            }
        }

        return entity;
    }

    public Room CurrentRoom { get; private set; }
    public RuleComputer Rules { get; }

    public GridPosition CameraPosition { get; set; }

    public event Action<string>? RequestLoad;
    public event Action<string>? RequestZoneNameChange;
    public event Action<string>? RequestShow;

    private Entity CreateTriggerEntityFromTemplate(PlacedEntity placedEntity)
    {
        var splitCommand = placedEntity.ExtraState.GetValueOrDefault("command")?.Split() ?? [];
        var entity = new Entity(placedEntity.Position, new Invisible());

        if (splitCommand.Length >= 2)
        {
            var commandName = splitCommand[0];
            var remainingArgs = splitCommand.ToList();
            remainingArgs.RemoveAt(0);
            var arg = string.Join(" ", remainingArgs);

            if (commandName == "load")
            {
                entity.AddBehavior(new EntityBehavior(BehaviorTrigger.OnTouch, () => { RequestLoad?.Invoke(arg); }));
            }

            if (commandName == "zone")
            {
                entity.AddBehavior(new EntityBehavior(BehaviorTrigger.OnEnter,
                    () => { RequestZoneNameChange?.Invoke(arg); }));
            }

            if (commandName == "show")
            {
                entity.AddBehavior(new EntityBehavior(BehaviorTrigger.OnTouch, () => { RequestShow?.Invoke(arg); }));
            }
        }

        return entity;
    }

    public void SetCurrentRoom(Room room)
    {
        CurrentRoom = room;
        CameraPosition = room.TopLeftPosition;
        foreach (var entity in room.AllActiveEntities())
        {
            entity.TriggerBehavior(BehaviorTrigger.OnEnter);
        }
    }

    public IEnumerable<Entity> AllEntitiesIncludingInactive()
    {
        return _entities;
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
        _entities.Add(entity);
        entity.Start();
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
        return new Room(this, topLeft, topLeft + _roomSize);
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
        foreach (var entity in CurrentRoom.AllActiveEntities().Where(a => a.Position == moveData.Destination))
        {
            entity.TriggerBehavior(BehaviorTrigger.OnTouch);
        }

        MoveCompleted?.Invoke(moveData, status);
    }

    public void Remove(Entity entity)
    {
        _entitiesToRemove.Add(entity);
    }

    public void UpdateEntityList()
    {
        foreach (var entity in _entitiesToRemove)
        {
            _entities.Remove(entity);
        }

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
}
