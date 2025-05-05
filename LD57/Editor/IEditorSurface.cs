using System;
using ExplogineMonoGame;
using LD57.Rendering;

namespace LD57.Editor;

public interface IEditorSurface
{
    string? FileName { get; set; }
    void Save(string surfaceFileName);
    void PaintWorldToScreen(AsciiScreen screen, GridPosition cameraPosition, float dt);
    void PaintOverlayBelowTool(AsciiScreen screen, GridPosition cameraPosition, GridPosition? hoveredWorldPosition);
    void PaintOverlayAboveTool(AsciiScreen screen, GridPosition cameraPosition);
    void Open(string path, bool isFullPath);
    void Clear();
    void HandleKeyBinds(ConsumableInput input);

    public event Action? RequestResetCamera;
}
