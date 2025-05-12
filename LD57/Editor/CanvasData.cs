using System;
using System.Linq;
using LD57.Rendering;

namespace LD57.Editor;

[Serializable]
public class CanvasData : EditorData<PlacedCanvasTile, CanvasTileData>
{
    private readonly CanvasBrushMode _canvasBrushMode;

    public CanvasData(CanvasBrushMode canvasBrushMode)
    {
        _canvasBrushMode = canvasBrushMode;
    }
    
    public override void PlaceInkAt(GridPosition position, CanvasTileData template)
    {
        var existingTile = AllInkAt(position).FirstOrDefault();

        if (existingTile == null && !_canvasBrushMode.ForegroundShapeAndTransform.IsVisibleAndEditing)
        {
            return;
        }
        
        template = _canvasBrushMode.Combine(existingTile?.CanvasTileData ?? new(), template);
        
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
