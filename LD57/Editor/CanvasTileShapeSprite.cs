using LD57.Rendering;

namespace LD57.Editor;

public class CanvasTileShapeSprite : ICanvasTileShape
{
    public CanvasTileShapeSprite(string sheetName, int frame)
    {
        SheetName = sheetName;
        Frame = frame;
    }

    public string SheetName { get; }
    public int Frame { get; }

    public TileState GetTileState()
    {
        return TileState.Sprite(ResourceAlias.GetSpriteSheetByName(SheetName) ?? ResourceAlias.Entities, Frame);
    }
}
