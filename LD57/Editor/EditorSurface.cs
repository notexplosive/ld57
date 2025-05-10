using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

public abstract class EditorSurface<TData, TPlaced, TInk> : IEditorSurface
    where TData : EditorData<TPlaced, TInk>, new()
    where TPlaced : IPlacedObject<TPlaced>
{
    private readonly string _resourceSubDirectory;

    protected EditorSurface(string resourceSubDirectory)
    {
        _resourceSubDirectory = resourceSubDirectory;
    }

    public TData Data { get; private set; } = new();
    protected abstract EditorSelection<TPlaced> RealSelection { get; }
    public event Action? RequestResetCamera;
    public string? FileName { get; set; }

    public IEditorSelection Selection => RealSelection;

    public void Save(string fileName)
    {
        Constants.WriteJsonToResources(Data, _resourceSubDirectory, fileName);
    }

    public abstract void PaintWorldToScreen(AsciiScreen screen, float dt);

    public abstract void PaintOverlayBelowTool(AsciiScreen screen,
        GridPosition? hoveredWorldPosition);

    public abstract void PaintOverlayAboveTool(AsciiScreen screen);

    public abstract void Open(string path, bool isFullPath);

    public void ClearEverything()
    {
        SetTemplateAndFileName(null, new TData());
    }

    public abstract void HandleKeyBinds(ConsumableInput input);

    public void RemoveInkAt(GridPosition position)
    {
        Data.RemoveEntitiesAt(position);
    }

    public bool HasContentAt(GridPosition position)
    {
        return Data.HasEntityAt(position);
    }

    public void MoveSelection()
    {
        foreach (var item in Selection.AllPositions())
        {
            Data.RemoveEntitiesAt(item);
        }

        foreach (var item in RealSelection.AllEntitiesWithCurrentPlacement())
        {
            Data.AddExact(item);
        }

        Selection.RegenerateAtNewPosition();
    }

    public void EraseSelection()
    {
        Data.EraseAtPositions(Selection.AllPositions());
    }

    protected void SetTemplateAndFileName(string? fileName, TData template)
    {
        FileName = fileName;
        Data = template;
        RequestResetCamera?.Invoke();
    }

    public IEnumerable<TPlaced> AllItemsAt(GridPosition position)
    {
        return Data.AllEntitiesAt(position);
    }
}
