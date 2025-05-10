using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public interface IEditorSurface
{
    public event Action? RequestResetCamera;
    string? FileName { get; set; }
    public IEditorSelection Selection { get; }
    void Save(string fileName);
    void PaintWorldToScreen(AsciiScreen screen, float dt);
    void PaintOverlayBelowTool(AsciiScreen screen, GridPosition? hoveredWorldPosition);
    void PaintOverlayAboveTool(AsciiScreen screen);
    void Open(string path, bool isFullPath);
    void ClearEverything();
    void HandleKeyBinds(ConsumableInput input);
    void RemoveInkAt(GridPosition position);
    bool HasContentAt(GridPosition position);
    void MoveSelection();
    void EraseSelection();
}
