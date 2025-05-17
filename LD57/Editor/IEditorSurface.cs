using System;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public interface IEditorSurface
{
    string? FileName { get; set; }
    public IEditorSelection Selection { get; }
    public event Action? RequestedResetCamera;
    public event Action<CreatePopupDelegate>? RequestedPopup;
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
    void RequestPopup(CreatePopupDelegate createPopup);
    void OnMiddleClickInWorld(GridPosition position);
}