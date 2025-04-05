using ExplogineMonoGame.AssetManagement;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public readonly record struct TileState(
    TileType TileType,
    Color Color,
    string? Character = null,
    SpriteSheet? SpriteSheet = null,
    int Frame = 0)
{
    public static TileState Glyph(string content, Color? color = null)
    {
        return new TileState(TileType.Character, color ?? Color.White, content);
    }

    public static TileState Sprite(SpriteSheet spriteSheet, int frame, Color? color = null)
    {
        return new TileState(TileType.Sprite, color ?? Color.White, null, spriteSheet, frame);
    }

    public static readonly TileState Empty = new TileState(TileType.Empty, Color.White); 
}
