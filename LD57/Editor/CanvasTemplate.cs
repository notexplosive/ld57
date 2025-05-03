using System;
using System.Collections.Generic;
using LD57.Rendering;
using Newtonsoft.Json;

namespace LD57.Editor;

[Serializable]
public class CanvasTemplate : EditableTemplate<CanvasTile>
{
    [JsonProperty("positions_to_tiles")]
    private Dictionary<string, CanvasTile> _tiles = new();

    public override void PlaceItemAt(GridPosition position, CanvasTile template)
    {
        _tiles[GridPositionToKey(position)] = template;
    }

    private static string GridPositionToKey(GridPosition position)
    {
        return position.X + "," + position.Y;
    }

    public override void EraseAt(GridPosition position)
    {
        _tiles.Remove(GridPositionToKey(position));
    }
}
