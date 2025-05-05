using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public interface IEditorSurface
{
    public event Action? RequestResetCamera;
    string? FileName { get; set; }
    void Save(string surfaceFileName);
    void PaintWorldToScreen(AsciiScreen screen, GridPosition cameraPosition, float dt);
    void PaintOverlayBelowTool(AsciiScreen screen, GridPosition cameraPosition, GridPosition? hoveredWorldPosition);
    void PaintOverlayAboveTool(AsciiScreen screen, GridPosition cameraPosition);
    void Open(string path, bool isFullPath);
    void Clear();
    void HandleKeyBinds(ConsumableInput input);
    public IEditorSelection Selection { get; }
    void RemoveEntitiesAt(GridPosition position);
    bool HasEntityAt(GridPosition position);
    void MoveSelection();
    void EraseAtPositions(IEnumerable<GridPosition> positions);
}
