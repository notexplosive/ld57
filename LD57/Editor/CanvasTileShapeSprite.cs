using LD57.Rendering;

namespace LD57.Editor;

public record CanvasTileShapeSprite(string SheetName, int Frame) : ICanvasTileShape
{
    public TileState GetTileState()
    {
        return TileState.Sprite(ResourceAlias.GetSpriteSheetByName(SheetName) ?? ResourceAlias.Entities, Frame);
    }
}
