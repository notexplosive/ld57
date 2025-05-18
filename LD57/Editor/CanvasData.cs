using System;
using System.Linq;
using LD57.Rendering;

namespace LD57.Editor;

[Serializable]
public class CanvasData : EditorData<PlacedCanvasTile, CanvasTileData, CanvasBrushFilter>
{
    public override void PlaceInkAt(GridPosition position, CanvasTileData tileData, CanvasBrushFilter canvasBrushFilter)
    {
        var existingTile = InkAt(position);

        if (existingTile == null && !canvasBrushFilter.ForegroundShapeAndTransform.IsFunctionallyActive)
        {
            return;
        }

        tileData = canvasBrushFilter.Combine(existingTile?.CanvasTileData ?? new CanvasTileData(), tileData);

        EraseAt(position);

        var ink = InkAt(position);
        if (ink != null && ink.CanvasTileData.HasExtraData())
        {
            tileData = tileData with {ExtraData = ink.CanvasTileData.ExtraData};
            
            // replace existing data
            ink.CanvasTileData = tileData;
        }
        else
        {
            // place new data
            Content.Add(new PlacedCanvasTile
            {
                Position = position,
                CanvasTileData = tileData
            });
        }
    }

    public PlacedCanvasTile? InkAt(GridPosition position)
    {
        return AllInkAt(position).FirstOrDefault();
    }

    public override void EraseAt(GridPosition position)
    {
        var items = Content.Where(a => a.Position == position).ToList();

        foreach (var item in items)
        {
            Content.Remove(item);
        }

        var tileWithMetadata = items.FirstOrDefault(a => a.CanvasTileData.HasExtraData());
        if (tileWithMetadata != null)
        {
            Content.Add(new PlacedCanvasTile
            {
                Position = position,
                CanvasTileData = new CanvasTileData {ExtraData = tileWithMetadata.CanvasTileData.ExtraData}
            });
        }
    }
}
