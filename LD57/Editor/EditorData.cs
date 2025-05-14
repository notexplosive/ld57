using System.Collections.Generic;
using System.Linq;
using LD57.Gameplay;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Editor;

public interface IBrushFilter
{
    
}

public abstract class EditorData<TPlaced, TInk, TFilter>
    where TPlaced : IPlacedObject<TPlaced>
    where TFilter : IBrushFilter
{
    [JsonProperty("content")]
    public List<TPlaced> Content = new();

    public void RemoveEntitiesAt(GridPosition position)
    {
        Content.RemoveAll(a => a.Position == position);
    }

    public IEnumerable<TPlaced> AllInkAt(GridPosition position)
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
    
    public void FillRectangle(GridRectangle rectangle, TInk ink, TFilter filter)
    {
        foreach (var position in rectangle.AllPositions())
        {
            PlaceInkAt(position, ink, filter);
        }
    }

    public void FillAllPositions(IEnumerable<GridPosition> positions, TInk ink, TFilter filter)
    {
        foreach (var position in positions)
        {
            PlaceInkAt(position, ink, filter);
        }
    }

    public abstract void PlaceInkAt(GridPosition position, TInk template, TFilter filter);

    public void EraseAllPositions(IEnumerable<GridPosition> allPositions)
    {
        foreach (var position in allPositions)
        {
            EraseAt(position);
        }
    }

    public abstract void EraseAt(GridPosition position);

    public bool HasInkAt(GridPosition position)
    {
        return Content.Any(a => a.Position == position);
    }
}
