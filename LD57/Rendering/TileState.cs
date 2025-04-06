using ExplogineMonoGame.AssetManagement;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public readonly record struct TileState(
    TileType TileType,
    Color ForegroundColor,
    string? Character = null,
    SpriteSheet? SpriteSheet = null,
    int Frame = 0,
    Color? BackgroundColor = null
)
{
    public static readonly TileState Empty = new(TileType.Empty, Color.White);

    public static TileState StringCharacter(string content, Color? color = null)
    {
        return new TileState(TileType.Character, color ?? Color.White, content);
    }

    public static TileState Sprite(SpriteSheet spriteSheet, int frame, Color? color = null)
    {
        return new TileState(TileType.Sprite, color ?? Color.White, null, spriteSheet, frame);
    }
}
