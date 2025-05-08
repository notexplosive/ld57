using System.Collections.Generic;
using System.Linq;
using LD57.Gameplay;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Editor;

public abstract class EditorData<TPlaced, TInk> where TPlaced : IPlacedObject<TPlaced>
{
    [JsonProperty("content")]
    public List<TPlaced> Content = new();

    public void RemoveEntitiesAt(GridPosition position)
    {
        Content.RemoveAll(a => a.Position == position);
    }

    public IEnumerable<TPlaced> AllEntitiesAt(GridPosition position)
    {
        foreach (var entity in Content)
        {
            if (entity.Position == position)
            {
                yield return entity;
            }
        }
    }

    public void RemoveExact(TPlaced entity)
    {
        Content.Remove(entity);
    }

    public void AddExact(TPlaced item)
    {
        Content.Add(item);
    }

    public void FillAllPositions(IEnumerable<GridPosition> positions, TInk ink)
    {
        foreach (var position in positions)
        {
            PlaceInkAt(position, ink);
        }
    }

    public abstract void PlaceInkAt(GridPosition position, TInk template);

    public void EraseAtPositions(IEnumerable<GridPosition> allPositions)
    {
        foreach (var position in allPositions)
        {
            EraseAt(position);
        }
    }

    public abstract void EraseAt(GridPosition position);

    public bool HasEntityAt(GridPosition position)
    {
        return Content.Any(a => a.Position == position);
    }
}
