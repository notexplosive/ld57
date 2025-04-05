using ExplogineMonoGame.AssetManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public record EntityAppearance(SpriteSheet SpriteSheet, int Frame, Color Color) : IEntityAppearance
{
    public TileState TileState => TileState.Sprite(SpriteSheet, Frame, Color);
}
