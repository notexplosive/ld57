using System;
using System.Collections.Generic;
using System.IO;
using ExplogineCore;
using ExplogineMonoGame;
using LD57.Gameplay;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Editor;

public abstract class EditorSurface<TData, TPlaced, TInk, TFilter> : IEditorSurface
    where TData : EditorData<TPlaced, TInk, TFilter>
    where TPlaced : IPlacedObject<TPlaced>
    where TFilter : IBrushFilter
{
    private readonly string _resourceSubDirectory;

    protected EditorSurface(string resourceSubDirectory, TData data)
    {
        _resourceSubDirectory = resourceSubDirectory;
        Data = data;
    }

    public TData Data { get; private set; }
    protected abstract EditorSelection<TPlaced> RealSelection { get; }
    public event Action? RequestedResetCamera;
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

    public void Open(string path, bool isFullPath)
    {
        string fileName;
        string json;

        if (isFullPath)
        {
            json = Client.Debug.RepoFileSystem.ReadFile(path);
            fileName = new FileInfo(path).Name;
        }
        else
        {
            json = Client.Debug.RepoFileSystem.GetDirectory($"Resource/{_resourceSubDirectory}")
                .ReadFile(path + ".json");
            fileName = path;
        }

        var data = JsonConvert.DeserializeObject<TData>(json);

        if (data != null)
        {
            SetTemplateAndFileName(fileName.RemoveFileExtension(), data);
        }
    }

    public void ClearEverything()
    {
        SetTemplateAndFileName(null, CreateEmptyData());
    }

    public abstract void HandleKeyBinds(ConsumableInput input);

    public void RemoveInkAt(GridPosition position)
    {
        Data.RemoveEntitiesAt(position);
    }

    public bool HasContentAt(GridPosition position)
    {
        return Data.HasInkAt(position);
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
        Data.EraseAllPositions(Selection.AllPositions());
    }

    public void RequestPopup(CreatePopupDelegate createPopup)
    {
        RequestedPopup?.Invoke(createPopup);
    }

    public void OnMiddleClickInWorld(GridPosition position)
    {
        RequestedEyeDropper?.Invoke(position);
    }

    public event Action<CreatePopupDelegate>? RequestedPopup;
    public event Action<GridPosition>? RequestedEyeDropper;

    protected abstract TData CreateEmptyData();

    private void SetTemplateAndFileName(string? fileName, TData template)
    {
        FileName = fileName;
        Data = template;
        RequestedResetCamera?.Invoke();
    }

    public IEnumerable<TPlaced> AllInkAt(GridPosition position)
    {
        return Data.AllInkAt(position);
    }
}
