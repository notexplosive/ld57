using LD57.Rendering;

namespace LD57.Editor;

public class CanvasTileShape : ICanvasTileShape
{
    public string SheetName { get; }
    public int Frame { get; }

    public CanvasTileShape(string sheetName, int frame)
    {
        SheetName = sheetName;
        Frame = frame;
    }

    public TileState TileState()
    {
        return Rendering.TileState.Sprite(ResourceAlias.GetSpriteSheetByName(SheetName) ?? ResourceAlias.Entities, Frame);
    }
}
