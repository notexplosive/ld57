using System;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

[Serializable]
public class CanvasData : EditorData<PlacedCanvasTile, CanvasTileData>
{
    public override void PlaceInkAt(GridPosition position, CanvasTileData template)
    {
        EraseAt(position);
        
        Content.Add(new PlacedCanvasTile
        {
            Position = position,
            CanvasTileData = template
        });
    }

    public override void EraseAt(GridPosition position)
    {
        Content.RemoveAll(a => a.Position == position);
    }
}
