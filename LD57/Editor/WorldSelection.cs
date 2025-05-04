using System.Collections.Generic;
using System.Linq;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class WorldSelection
{
    private readonly HashSet<PlacedEntity> _placedEntitiesFromWorld = new();
    private readonly HashSet<GridPosition> _startingPositions = new();

    public GridPosition Offset { get; set; }

    public bool IsEmpty => _startingPositions.Count == 0 && _placedEntitiesFromWorld.Count == 0;

    public IEnumerable<GridPosition> AllPositions()
    {
        foreach (var position in _startingPositions)
        {
            yield return position + Offset;
        }
    }

    public void Clear()
    {
        Offset = new GridPosition();
        _placedEntitiesFromWorld.Clear();
        _startingPositions.Clear();
    }

    private void AddEntities(IEnumerable<PlacedEntity> entities)
    {
        foreach (var entity in entities)
        {
            _placedEntitiesFromWorld.Add(entity);
        }
    }
    
    private void RemoveEntities(IEnumerable<PlacedEntity> entities)
    {
        foreach (var entity in entities)
        {
            _placedEntitiesFromWorld.Remove(entity);
        }
    }

    public string Status()
    {
        return $"{_placedEntitiesFromWorld.Count} selected";
    }

    public IEnumerable<PlacedEntity> AllEntitiesWithCurrentPlacement()
    {
        foreach (var entity in _placedEntitiesFromWorld)
        {
            yield return entity.MovedBy(Offset);
        }
    }

    public bool Contains(GridPosition gridPosition)
    {
        return AllPositions().Contains(gridPosition);
    }

    public void RemovePositions(EditorSession editorSession, IEnumerable<GridPosition> positions)
    {
        foreach (var position in positions)
        {
            RemovePosition(editorSession, position);
        }
    }

    public void AddPositions(EditorSession editorSession, IEnumerable<GridPosition> positions)
    {
        foreach (var position in positions)
        {
            AddPosition(editorSession, position);
        }
    }

    private void AddPosition(EditorSession editorSession, GridPosition position)
    {
        AddEntities(editorSession.Surface.WorldTemplate.AllEntitiesAt(position));
        _startingPositions.Add(position);
    }
    
    private void RemovePosition(EditorSession editorSession, GridPosition position)
    {
        RemoveEntities(editorSession.Surface.WorldTemplate.AllEntitiesAt(position));
        _startingPositions.Remove(position);
    }

    

    public void RegenerateAtNewPosition(EditorSession editorSession)
    {
        var currentPositions = AllPositions().ToList();
        Clear();
        AddPositions(editorSession, currentPositions);
    }

    public TileState GetTileState(GridPosition internalPosition)
    {
        var entities = _placedEntitiesFromWorld.Where(a => a.Position == internalPosition);
        var topTemplate = entities.Select(entity => ResourceAlias.EntityTemplate(entity.TemplateName) ?? new EntityTemplate())
            .OrderBy(a => a.SortPriority).FirstOrDefault();

        if (topTemplate == null)
        {
            return TileState.BackgroundOnly(Color.Goldenrod, 1f);
        }

        return topTemplate.CreateAppearance().TileState with
        {
            ForegroundColor = Color.DarkGoldenrod,
            BackgroundColor = Color.Goldenrod,
            BackgroundIntensity = 1f
        };
    }
}
