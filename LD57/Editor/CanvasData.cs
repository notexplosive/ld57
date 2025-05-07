using System;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

[Serializable]
public class CanvasData : EditorData<PlacedCanvasTile, CanvasTileData>
{
    protected override void PlaceInkAt(CanvasTileData template, GridPosition position)
    {
        Content.Add(new PlacedCanvasTile
        {
            Position = position,
            CanvasTileData = template
        });
    }

    protected override void EraseAt(GridPosition position)
    {
        Content.RemoveAll(a => a.Position == position);
    }
}
