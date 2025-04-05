using System;
using System.Collections.Generic;
using LD57.Rendering;

namespace LD57.Gameplay;

public class World
{
    private readonly List<Entity> _entities = new();
    private readonly GridPosition _roomSize;
    private readonly HashSet<Entity> _entitiesToRemove = new();

    public World(GridPosition roomSize)
    {
        _roomSize = roomSize;
        Rules = new RuleComputer(this);
        var gridPosition = new GridPosition(0, 0);
        CurrentRoom = new Room(this, gridPosition, gridPosition + roomSize);
    }

    public Room CurrentRoom { get; set; }
    public RuleComputer Rules { get; }

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

    public Entity AddEntity(Entity entity)
    {
        _entities.Add(entity);
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
}
