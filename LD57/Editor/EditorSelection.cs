using System.Collections.Generic;
using System.Linq;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

public abstract class EditorSelection<TPlaced> : IEditorSelection
    where TPlaced : IPlacedObject<TPlaced>
{
    private readonly HashSet<GridPosition> _startingPositions = new();
    protected readonly HashSet<TPlaced> PlacedObjects = new();
    public GridPosition Offset { get; set; }
    public bool IsEmpty => _startingPositions.Count == 0 && PlacedObjects.Count == 0;

    public void Clear()
    {
        Offset = new GridPosition();
        PlacedObjects.Clear();
        _startingPositions.Clear();
    }

    public string Status()
    {
        return $"{PlacedObjects.Count} selected";
    }

    public bool Contains(GridPosition gridPosition)
    {
        return AllPositions().Contains(gridPosition);
    }

    public void RemovePositions(IEnumerable<GridPosition> positions)
    {
        foreach (var position in positions)
        {
            RemovePosition(position);
        }
    }

    public void AddPositions(IEnumerable<GridPosition> positions)
    {
        foreach (var position in positions)
        {
            AddPosition(position);
        }
    }

    public void AddPosition(GridPosition position)
    {
        AddObjects(GetAllObjectsAt(position));
        _startingPositions.Add(position);
    }

    public void RemovePosition(GridPosition position)
    {
        RemoveObjects(GetAllObjectsAt(position));
        _startingPositions.Remove(position);
    }

    public abstract TileState GetTileState(GridPosition internalPosition);

    public IEnumerable<GridPosition> AllPositions()
    {
        foreach (var position in _startingPositions)
        {
            yield return position + Offset;
        }
    }

    public void RegenerateAtNewPosition()
    {
        var currentPositions = AllPositions().ToList();
        Clear();
        AddPositions(currentPositions);
    }

    protected abstract IEnumerable<TPlaced> GetAllObjectsAt(GridPosition position);

    private void AddObjects(IEnumerable<TPlaced> entities)
    {
        foreach (var entity in entities)
        {
            PlacedObjects.Add(entity);
        }
    }

    private void RemoveObjects(IEnumerable<TPlaced> objects)
    {
        foreach (var entity in objects)
        {
            PlacedObjects.Remove(entity);
        }
    }

    /// <summary>
    /// Gets copies of all objects moved by the Offset
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TPlaced> AllEntitiesWithCurrentPlacement()
    {
        foreach (var entity in PlacedObjects)
        {
            yield return entity.MovedBy(Offset);
        }
    }
}
