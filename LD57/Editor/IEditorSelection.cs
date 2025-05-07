using System.Collections.Generic;
using LD57.Rendering;

namespace LD57.Editor;

public interface IEditorSelection
{
    public bool IsEmpty { get; }
    public GridPosition Offset { get; set; }
    public void Clear();
    public string Status();
    public bool Contains(GridPosition gridPosition);
    public void RemovePositions(IEnumerable<GridPosition> positions);
    public void AddPositions(IEnumerable<GridPosition> positions);
    public IEnumerable<GridPosition> AllPositions();
    public void AddPosition(GridPosition position);
    public void RemovePosition(GridPosition position);
    public TileState GetTileStateAt(GridPosition internalPosition);
    public void RegenerateAtNewPosition();
}
