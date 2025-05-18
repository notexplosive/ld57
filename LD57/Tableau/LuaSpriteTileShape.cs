using System;
using ExplogineCore.Data;
using ExplogineCore.Lua;
using ExplogineMonoGame.AssetManagement;
using LD57.Core;
using LD57.Rendering;

namespace LD57.Tableau;

[LuaBoundType]
public readonly record struct LuaSpriteTileShape : ILuaTileShape
{
    public LuaSpriteTileShape(string sheetName, int frame, float angle, bool flipX, bool flipY)
    {
        Sheet = ResourceAlias.GetSpriteSheetByName(sheetName) ??
                throw new Exception($"Could not load sprite sheet: `{sheetName}`");
        SheetName = sheetName;
        Frame = frame;
        Rotation = angle;
        FlipXy = new XyBool(flipX, flipY);
    }

    public XyBool FlipXy { get; }
    public string SheetName { get; }
    private int Frame { get; }
    public float Rotation { get; }
    private SpriteSheet Sheet { get; }

    public TileState GetTileState()
    {
        return TileState.Sprite(Sheet, Frame) with {Angle = Rotation, Flip = FlipXy};
    }
}
