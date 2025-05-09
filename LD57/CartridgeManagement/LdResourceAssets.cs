using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExplogineCore;
using ExplogineCore.Aseprite;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using FontStashSharp;
using LD57.Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace LD57.CartridgeManagement;

public class LdResourceAssets
{
    private static LdResourceAssets? instanceImpl;

    private readonly Dictionary<string, Canvas> _dynamicTextures = new();
    public Dictionary<string, Color?> NamedColors { get; } = new();
    private readonly object _soundLock = new();

    public static LdResourceAssets Instance => instanceImpl ??= new LdResourceAssets();

    public Dictionary<string, SpriteSheet?> Sheets { get; } = new();
    public Dictionary<string, SoundEffectInstance> SoundInstances { get; set; } = new();
    public Dictionary<string, SoundEffect> SoundEffects { get; set; } = new();
    public Dictionary<string, byte[]> RawFontBytes { get; } = new();
    public Dictionary<string, FontSystem> FontSystems { get; } = new();
    public Dictionary<string, EntityTemplate> EntityTemplates { get; } = new();
    public Dictionary<string, MessageContent> Messages { get; } = new();

    public IEnumerable<Color> AllKnownColors
    {
        get
        {
            foreach (var color in NamedColors.Values)
            {
                if (color.HasValue)
                {
                    yield return color.Value;
                }
            }
        }
    }

    public static Color MissingColor { get; } = new(255, 0, 255);

    public IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        var resourceFiles = Client.Debug.RepoFileSystem.GetDirectory("Resource");

        yield return new VoidLoadEvent("sprite-atlas", "Sprite Atlas", () =>
        {
            var texturePath = Path.Join(resourceFiles.GetCurrentDirectory(), "atlas.png");
            if (!File.Exists(texturePath))
            {
                return;
            }

            var texture = Texture2D.FromFile(Client.Graphics.Device, texturePath);
            var sheetInfo = JsonConvert.DeserializeObject<AsepriteSheetData>(resourceFiles.ReadFile("atlas.json"));

            if (sheetInfo != null)
            {
                foreach (var frame in sheetInfo.Frames)
                {
                    // Remove extension
                    var splitSheetName =
                        frame.Key
                            .Replace(".aseprite", "")
                            .Replace(".ase", "")
                            .Replace(".png", "")
                            .Split(" ").ToList();

                    if (splitSheetName.Count > 1)
                    {
                        // If there is a number suffix, remove it
                        splitSheetName.RemoveAt(splitSheetName.Count - 1);
                    }

                    var sheetName = string.Join(" ", splitSheetName);
                    if (!Sheets.ContainsKey(sheetName))
                    {
                        Sheets.Add(sheetName, new SelectFrameSpriteSheet(texture));
                    }

                    var rect = frame.Value.Frame;
                    (Sheets[sheetName] as SelectFrameSpriteSheet)!.AddFrame(new Rectangle(rect.X, rect.Y, rect.Width,
                        rect.Height));
                }
            }
        });

        yield return new ThreadedVoidLoadEvent("Sounds", "Sounds", () =>
        {
            foreach (var path in resourceFiles.GetFilesAt(".", "ogg"))
            {
                AddSound(resourceFiles, path);
            }
        });

        yield return new VoidLoadEvent("Fonts", () =>
        {
            foreach (var path in resourceFiles.GetFilesAt(".", "ttf"))
            {
                AddFont(resourceFiles, path);
            }
        });
    }

    public void Unload()
    {
        Unload(_dynamicTextures);
        Unload(SoundEffects);
        Unload(SoundInstances);
        Unload(FontSystems);
        RawFontBytes.Clear();
        Messages.Clear();
        EntityTemplates.Clear();
    }

    private void Unload<T>(Dictionary<string, T> dictionary) where T : IDisposable
    {
        foreach (var sound in dictionary.Values)
        {
            sound.Dispose();
        }

        dictionary.Clear();
    }

    public void AddDynamicSpriteSheet(string key, Point size, Action generateTexture,
        Func<Texture2D, SpriteSheet> generateSpriteSheet)
    {
        if (_dynamicTextures.ContainsKey(key))
        {
            _dynamicTextures[key].Dispose();
            _dynamicTextures.Remove(key);
        }

        var canvas = new Canvas(size.X, size.Y);
        _dynamicTextures.Add(key, canvas);

        Client.Graphics.PushCanvas(canvas);
        generateTexture();
        Client.Graphics.PopCanvas();

        Sheets.Add(key, generateSpriteSheet(canvas.Texture));
    }

    public void AddSpriteSheet(string key, SpriteSheet spriteSheet)
    {
        Sheets[key] = spriteSheet;
    }

    private void AddSound(IFileSystem resourceFiles, string path)
    {
        var vorbis = ReadOgg.ReadVorbis(Path.Join(resourceFiles.GetCurrentDirectory(), path));
        var soundEffect = ReadOgg.ReadSoundEffect(vorbis);
        var key = path.RemoveFileExtension();
        var instance = soundEffect.CreateInstance();

        lock (_soundLock)
        {
            SoundInstances[key] = instance;
            SoundEffects[key] = soundEffect;
        }
    }

    private void AddFont(IFileSystem resourceFiles, string path)
    {
        var bytes = resourceFiles.ReadBytes(path);
        var key = path.RemoveFileExtension();
        RawFontBytes[key] = bytes;
        var fontSystem = new FontSystem();
        fontSystem.AddFont(bytes);
        FontSystems[key] = fontSystem;
    }

    public void PlaySound(string key, SoundEffectSettings settings)
    {
        if (SoundInstances.TryGetValue(key, out var sound))
        {
            if (settings.Cached)
            {
                sound.Stop();
            }

            sound.Pan = settings.Pan;
            sound.Pitch = settings.Pitch;
            sound.Volume = settings.Volume;
            sound.IsLooped = settings.Loop;

            sound.Play();
        }
        else
        {
            Client.Debug.LogWarning($"Could not find sound `{key}`");
        }
    }

    public static void Reset()
    {
        Instance.Unload();
        instanceImpl = null;
    }

    public void AddKnownColors(Dictionary<string, string> colorTable)
    {
        foreach (var keyValuePair in colorTable)
        {
            NamedColors.Add(keyValuePair.Key, ColorExtensions.FromRgbaHexString(keyValuePair.Value));
        }
    }

    public bool HasNamedColor(string name)
    {
        return NamedColors.ContainsKey(name);
    }

    public Color GetNamedColor(string name)
    {
        return NamedColors.GetValueOrDefault(name) ?? MissingColor;
    }
}
