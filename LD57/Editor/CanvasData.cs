using System;
using System.Linq;
using LD57.Rendering;

namespace LD57.Editor;

[Serializable]
public class CanvasData : EditorData<PlacedCanvasTile, CanvasTileData>
{
    private readonly CanvasBrushFilter _canvasBrushFilter;

    public CanvasData(CanvasBrushFilter canvasBrushFilter)
    {
        _canvasBrushFilter = canvasBrushFilter;
    }
    
    public override void PlaceInkAt(GridPosition position, CanvasTileData template)
    {
        var existingTile = InkAt(position);

        if (existingTile == null && !_canvasBrushFilter.ForegroundShapeAndTransform.IsFunctionallyActive)
        {
            return;
        }
        
        template = _canvasBrushFilter.Combine(existingTile?.CanvasTileData ?? new(), template);
        
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
