using ExplogineMonoGame.AssetManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class EntityAppearance : IEntityAppearance
{
    public EntityAppearance(SpriteSheet spriteSheet, int frame, Color color, int sortPriority)
    {
        SpriteSheet = spriteSheet;
        Frame = frame;
        Color = color;
        TileState = TileState.Sprite(SpriteSheet, Frame, Color);
        RawSortPriority = sortPriority;
    }

    public int RawSortPriority { get; }
    public SpriteSheet SpriteSheet { get; init; }
    public int Frame { get; init; }
    public Color Color { get; init; }

    public TileState TileState { get; set; }
}
