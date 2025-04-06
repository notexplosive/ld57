using System.Collections.Generic;
using System.Linq;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class Room
{
    private readonly GridPosition _bottomRight;
    private readonly List<Entity> _entitiesInRoom = new();
    private readonly GridPosition _topLeft;
    private readonly World _world;

    public Room(World parentWorld, GridPosition topLeft, GridPosition bottomRight)
    {
        _world = parentWorld;
        _topLeft = topLeft;
        _bottomRight = bottomRight;

        RecalculateLiveEntities();
    }

    public GridPosition TopLeftPosition => _topLeft;
    public GridPosition BottomRightPosition => _bottomRight;

    public Rectangle Rectangle => new(_topLeft.ToPoint(), (_bottomRight - _topLeft).ToPoint() + new Point(1));

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
        _entitiesInRoom.AddRange(CalculateEntitiesInRoom(false));
    }

    private IEnumerable<Entity> CalculateEntitiesInRoom(bool onlyActive)
    {
        var rectangle = new Rectangle(_topLeft.ToPoint(), (_bottomRight - _topLeft).ToPoint() + new Point(1));
        foreach (var entity in onlyActive ? _world.AllActiveEntities() : _world.AllEntitiesIncludingInactive())
        {
            if (rectangle.Contains(entity.Position.ToPoint()))
            {
                yield return entity;
            }
        }
    }

    public IEnumerable<GridPosition> AffectedCells()
    {
        var width = _bottomRight.X - _topLeft.X;
        var height = _bottomRight.Y - _topLeft.Y;

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                yield return new GridPosition(x, y);
            }
        }
    }

    public bool Contains(GridPosition newPosition)
    {
        return Rectangle.Contains(
            newPosition.ToPoint());
    }
}
