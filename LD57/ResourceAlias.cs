using ExplogineMonoGame.AssetManagement;
using FontStashSharp;
using LD57.CartridgeManagement;

namespace LD57;

public static class ResourceAlias
{
    public static FontSystem GameFont => LdResourceAssets.Instance.FontSystems["ConcertOne/ConcertOne-Regular"];
    public static SpriteSheet Walls => LdResourceAssets.Instance.Sheets["Walls"];
    public static SpriteSheet Floors => LdResourceAssets.Instance.Sheets["Floors"];
    public static SpriteSheet Entities => LdResourceAssets.Instance.Sheets["Entities"];
    public static SpriteSheet PopupFrame => LdResourceAssets.Instance.Sheets["PopupFrameParts"];
}
