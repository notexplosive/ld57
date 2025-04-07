using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class State
{
    public delegate void StateUpdateDelegate(string key, string value);

    private readonly Dictionary<string, string> _table = new();

    public event StateUpdateDelegate? Updated;

    public void SetString(string key, string value)
    {
        _table[key] = value;
        Updated?.Invoke(key, value);
    }

    public void Set<T>(string key, T data) where T : notnull
    {
        SetString(key, data.ToString() ?? "");
    }

    public void Remove(string key)
    {
        _table.Remove(key);
    }

    public int? GetInt(string key)
    {
        return GetAsTOrDefault(key, value =>
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }

            return new int?();
        });
    }

    public int GetIntOrFallback(string key, int fallback)
    {
        return GetInt(key) ?? fallback;
    }

    public Color? GetColor(string key)
    {
        return GetAsTOrDefault(key, value =>
        {
            if (ColorExtensions.TryFromRgbaHexString(value, out var result))
            {
                return result;
            }

            return new Color?();
        });
    }

    private T? GetAsTOrDefault<T>(string key, Func<string, T?> tryParse)
    {
        var stringResult = GetString(key);
        if (stringResult == null)
        {
            return default;
        }

        var parsed = tryParse(stringResult);
        if (parsed == null)
        {
            return default;
        }

        return parsed;
    }

    public string? GetString(string key)
    {
        return _table.GetValueOrDefault(key);
    }

    public bool? GetBool(string key)
    {
        return GetAsTOrDefault(key, value =>
        {
            if (bool.TryParse(value, out var result))
            {
                return result;
            }

            return new bool?();
        });
    }

    public void AddFromDictionary(Dictionary<string, string> extraData)
    {
        foreach (var data in extraData)
        {
            SetString(data.Key, data.Value);
        }
    }

    public bool GetBoolOrFallback(string key, bool defaultBool)
    {
        return GetBool(key) ?? defaultBool;
    }

    public string GetStringOrDefault(string key, string fallback)
    {
        return GetString(key) ?? fallback;
    }

    public bool HasKey(string key)
    {
        return _table.ContainsKey(key);
    }
}
