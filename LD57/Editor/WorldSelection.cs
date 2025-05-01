using System.Collections.Generic;
using System.Linq;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class WorldSelection
{
    private readonly List<PlacedEntity> _placedEntitiesFromWorld = new();
    private readonly List<GridPosition> _startingPositions = new();

    public GridPosition Offset { get; set; }

    public bool IsEmpty { get; private set; }

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
        IsEmpty = true;
    }

    public void AddEntities(IEnumerable<PlacedEntity> entities)
    {
        IsEmpty = false;
        _placedEntitiesFromWorld.AddRange(entities);
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

    public void AddRectangle(EditorSession editorSession, GridPositionCorners rectangle)
    {
        AddPositions(editorSession, rectangle.AllPositions(true));
    }

    private void AddPositions(EditorSession editorSession, IEnumerable<GridPosition> positions)
    {
        foreach (var position in positions)
        {
            AddPosition(editorSession, position);
        }
    }

    private void AddPosition(EditorSession editorSession, GridPosition position)
    {
        editorSession.WorldSelection.AddEntities(editorSession.WorldTemplate.AllEntitiesAt(position));
        _startingPositions.Add(position);
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
        var topTemplate = entities.Select(entity => ResourceAlias.EntityTemplate(entity.TemplateName))
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
