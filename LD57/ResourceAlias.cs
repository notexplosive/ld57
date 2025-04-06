using System;
using System.Collections.Generic;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using FontStashSharp;
using LD57.CartridgeManagement;
using LD57.Gameplay;
using Microsoft.Xna.Framework;

namespace LD57;

public static class ResourceAlias
{
    public static FontSystem GameFont => LdResourceAssets.Instance.FontSystems["ConcertOne/ConcertOne-Regular"];
    public static SpriteSheet Walls => LdResourceAssets.Instance.Sheets["Walls"];
    public static SpriteSheet Floors => LdResourceAssets.Instance.Sheets["Floors"];
    public static SpriteSheet Entities => LdResourceAssets.Instance.Sheets["Entities"];
    public static SpriteSheet PopupFrame => LdResourceAssets.Instance.Sheets["PopupFrameParts"];
    public static SpriteSheet Tools => LdResourceAssets.Instance.Sheets["Tools"];

    public static Color Color(string colorString)
    {
        if (LdResourceAssets.Instance.HasNamedColor(colorString))
        {
            return LdResourceAssets.Instance.GetNamedColor(colorString);
        }

        if (ColorExtensions.TryFromRgbaHexString(colorString, out var color))
        {
            return color.Value;
        }

        return LdResourceAssets.MissingColor;
    }

    public static EntityTemplate EntityTemplate(string name)
    {
        var result = LdResourceAssets.Instance.EntityTemplates.GetValueOrDefault(name);

        if (result == null)
        {
            throw new Exception($"Could not find template {name}");
        }

        return result;
    }
}
