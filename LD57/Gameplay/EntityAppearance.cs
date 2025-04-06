using ExplogineMonoGame.AssetManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class EntityAppearance : IEntityAppearance
{
    public EntityAppearance(SpriteSheet spriteSheet, int frame, Color color)
    {
        SpriteSheet = spriteSheet;
        Frame = frame;
        Color = color;
        TileState = Rendering.TileState.Sprite(SpriteSheet, Frame, Color);
    }

    public SpriteSheet SpriteSheet { get; init; }
    public int Frame { get; init; }
    public Color Color { get; init; }

    public TileState? TileState { get; set; }
}
