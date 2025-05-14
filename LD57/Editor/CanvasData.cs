using System;
using System.Linq;
using LD57.Rendering;

namespace LD57.Editor;

[Serializable]
public class CanvasData : EditorData<PlacedCanvasTile, CanvasTileData, CanvasBrushFilter>
{
    public override void PlaceInkAt(GridPosition position, CanvasTileData template, CanvasBrushFilter canvasBrushFilter)
    {
        var existingTile = InkAt(position);

        if (existingTile == null && !canvasBrushFilter.ForegroundShapeAndTransform.IsFunctionallyActive)
        {
            return;
        }
        
        template = canvasBrushFilter.Combine(existingTile?.CanvasTileData ?? new(), template);
        
        EraseAt(position);

        Content.Add(new PlacedCanvasTile
        {
            Position = position,
            CanvasTileData = template
        });
    }

    public PlacedCanvasTile? InkAt(GridPosition position)
    {
        return AllInkAt(position).FirstOrDefault();
    }

    public override void EraseAt(GridPosition position)
    {
        Content.RemoveAll(a => a.Position == position);
    }
}
