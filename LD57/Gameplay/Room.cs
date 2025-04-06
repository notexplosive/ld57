﻿using System.Collections.Generic;
using System.Linq;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class Room
{
    private readonly List<Entity> _entitiesInRoom = new();
    private readonly World _world;
    private readonly GridPositionCorners _corners;

    public Room(World parentWorld, GridPosition a, GridPosition b)
    {
        _world = parentWorld;
        _corners = new GridPositionCorners(a, b);

        RecalculateLiveEntities();
    }

    public GridPosition TopLeft => _corners.TopLeft;
    public GridPosition BottomRight => _corners.BottomRight;
    public Rectangle Rectangle => _corners.Rectangle(true);

    public IEnumerable<Entity> AllEntitiesIncludingInactive()
    {
        return _entitiesInRoom;
    }

    public IEnumerable<Entity> AllActiveEntities()
    {
        foreach (var entity in _entitiesInRoom)
        {
            if (entity.IsActive)
            {
                yield return entity;
            }
        }
    }

    /// <summary>
    ///     A "turn" is whatever arbitrary thing we decide (right now: whenever the player moves)
    /// </summary>
    public void TriggerTurn()
    {
        foreach (var entity in _entitiesInRoom)
        {
            entity.SelfTriggerBehavior(BehaviorTrigger.OnTurn);
        }
    }

    /// <summary>
    ///     Includes "Inactive" Entities
    /// </summary>
    public List<Entity> AllVisibleEntitiesInDrawOrder()
    {
        var list = AllEntitiesIncludingInactive().ToList();
        list.Sort((entityA, entityB) => entityB.SortPriority.CompareTo(entityA.SortPriority));
        return list;
    }

    public void RecalculateLiveEntities()
    {
        _entitiesInRoom.Clear();
        _entitiesInRoom.AddRange(_world.CalculateEntitiesInRoom(false, _corners));
    }

    public bool Contains(GridPosition position)
    {
        return Rectangle.Contains(position.ToPoint());
    }
}
