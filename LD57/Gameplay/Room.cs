using System.Collections.Generic;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class Room : IRoom
{
    private readonly List<Entity> _activeEntities = new();
    private readonly GridPosition _bottomRight;
    private readonly GridPosition _topLeft;
    private readonly World _world;

    public Room(World parentWorld, GridPosition topLeft, GridPosition bottomRight)
    {
        _world = parentWorld;
        _topLeft = topLeft;
        _bottomRight = bottomRight;

        CalculateLiveEntities();
    }

    public GridPosition TopLeftPosition => _topLeft;

    public IEnumerable<Entity> AllEntities()
    {
        return _activeEntities;
    }

    public void CalculateLiveEntities()
    {
        _activeEntities.Clear();

        var rectangle = new Rectangle(_topLeft.ToPoint(), (_bottomRight - _topLeft).ToPoint() + new Point(1));
        foreach (var entity in _world.AllEntities())
        {
            if (rectangle.Contains(entity.Position.ToPoint()))
            {
                _activeEntities.Add(entity);
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
        return new Rectangle(_topLeft.ToPoint(), (_bottomRight - _topLeft).ToPoint() + new Point(1)).Contains(newPosition.ToPoint());
    }
}
