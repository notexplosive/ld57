using ExplogineCore.Data;
using ExplogineMonoGame.AssetManagement;
using Microsoft.Xna.Framework;

namespace LD57.Rendering;

public readonly record struct TileState(
    TileType TileType,
    Color ForegroundColor,
    Color BackgroundColor,
    string? Character = null,
    SpriteSheet? SpriteSheet = null,
    int Frame = 0,
    float BackgroundIntensity = 0f,
    XyBool Flip = default,
    float Angle = 0
)
{
    public static readonly TileState TransparentEmpty = new(TileType.Skip, Color.White, Color.White);

    public static TileState StringCharacter(string content, Color? color = null)
    {
        return new TileState(TileType.Character, color ?? Color.White, Color.White, content);
    }

    public static TileState Sprite(SpriteSheet spriteSheet, int frame, Color? color = null)
    {
        return new TileState(TileType.Sprite, color ?? Color.White, Color.White, null, spriteSheet, frame);
    }

    public static TileState CombineLayers(TileState higher, TileState lower)
    {
        if (higher.TileType == TileType.Skip)
        {
            return lower;
        }

        if (lower.TileType == TileType.Skip)
        {
            return higher;
        }
        
        var backgroundColor = higher.BackgroundColor;
        var intensity = higher.BackgroundIntensity;

        if (!higher.HasBackground)
        {
            if (lower.HasBackground)
            {
                backgroundColor = lower.BackgroundColor;
                intensity = lower.BackgroundIntensity;
            }
            else
            {
                // use the lower's foreground as the background color
                backgroundColor = lower.ForegroundColor;
                
                // If no intensity is set for this case, use a default value
                intensity = 1f;
            }
        }

        return higher with
        {
            BackgroundColor = backgroundColor,
            BackgroundIntensity = intensity
        };
    }

    public bool HasBackground => BackgroundIntensity > 0;

    public static TileState BackgroundOnly(Color backgroundColor, float intensity)
    {
        return new TileState(TileType.Invisible, Color.White, Color.White) { BackgroundColor = backgroundColor, BackgroundIntensity = intensity };
    }

    public TileState WithBackground(Color color, float intensity = 1)
    {
        return this with {BackgroundColor = color, BackgroundIntensity = intensity};
    }
}
