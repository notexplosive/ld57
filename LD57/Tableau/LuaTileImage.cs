using System;
using System.Collections.Generic;
using ExplogineCore.Lua;
using ExplogineMonoGame;
using JetBrains.Annotations;
using LD57.Editor;
using LD57.Rendering;
using MoonSharp.Interpreter;
using Newtonsoft.Json;

namespace LD57.Tableau;

[LuaBoundType]
public record LuaTileImage
{
    private readonly Dictionary<GridPosition, CanvasTileData> _content = new();
    private readonly int _height;
    private readonly LuaRuntime _luaRuntime;
    private readonly int _width;

    public LuaTileImage(string fileName, LuaRuntime luaRuntime)
    {
        _luaRuntime = luaRuntime;
        var file = Client.Debug.RepoFileSystem.GetDirectory("Resource/Canvases").ReadFile(fileName + ".json");
        var data = JsonConvert.DeserializeObject<CanvasData>(file) ??
                   throw new Exception($"Could not load image: {fileName}");

        int? minX = null;
        int? minY = null;
        int? maxX = null;
        int? maxY = null;

        data.Content.Sort((a, b) =>
        {
            var x = a.Position.X.CompareTo(b.Position.X);
            var y = a.Position.Y.CompareTo(b.Position.Y);

            if (y != 0)
            {
                return y;
            }

            return x;
        });

        foreach (var item in data.Content)
        {
            if (!minX.HasValue || !maxX.HasValue || !maxY.HasValue || !minY.HasValue)
            {
                minX = item.Position.X;
                maxX = item.Position.X;
                minY = item.Position.Y;
                maxY = item.Position.Y;
            }

            _content.Add(item.Position, item.CanvasTileData);
            maxX = Math.Max(maxX.Value, item.Position.X);
            maxY = Math.Max(maxY.Value, item.Position.Y);

            minX = Math.Min(minX.Value, item.Position.X);
            minY = Math.Min(minY.Value, item.Position.Y);
        }

        if (minX.HasValue && maxX.HasValue && maxY.HasValue && minY.HasValue)
        {
            _width = maxX.Value - minX.Value;
            _height = maxY.Value - minY.Value;
        }
    }

    public void PaintToScreen(AsciiScreen screen, GridPosition gridPosition)
    {
        screen.PushTransform(gridPosition);

        foreach (var (position, tileData) in _content)
        {
            screen.PutTile(position, tileData.GetTileUnfiltered());
        }

        screen.PopTransform();
    }
    
    public void PaintToScreenSlice(AsciiScreen screen, GridPosition gridPosition, GridRectangle innerRect)
    {
        screen.PushTransform(gridPosition);

        foreach (var (position, tileData) in _content)
        {
            if (innerRect.Contains(position, true))
            {
                screen.PutTile(position - innerRect.TopLeft, tileData.GetTileUnfiltered());
            }
        }

        screen.PopTransform();
    }

    [UsedImplicitly]
    [LuaMember("positions")]
    public Table Positions()
    {
        var output = _luaRuntime.NewTable();

        foreach (var position in GetPositionsRaw())
        {
            var tableAsDynValue = _luaRuntime.NewTableAsDynValue();
            tableAsDynValue.Table["x"] = position.X;
            tableAsDynValue.Table["y"] = position.Y;
            output.Append(tableAsDynValue);
        }

        return output;
    }

    [UsedImplicitly]
    [LuaMember("getColorAt")]
    public string GetColorAt(int x, int y)
    {
        var position = new GridPosition(x, y);
        var tileData = _content.GetValueOrDefault(position);
        return tileData?.ForegroundColorName ?? "white";
    }

    [UsedImplicitly]
    [LuaMember("getBackgroundColorAt")]
    public string GetBackgroundColorAt(int x, int y)
    {
        var position = new GridPosition(x, y);
        var tileData = _content.GetValueOrDefault(position);
        return tileData?.BackgroundColorName ?? "white";
    }

    [UsedImplicitly]
    [LuaMember("getBackgroundIntensityAt")]
    public float GetBackgroundIntensityAt(int x, int y)
    {
        var position = new GridPosition(x, y);
        var tileData = _content.GetValueOrDefault(position);
        return tileData?.BackgroundIntensity ?? 0;
    }

    [UsedImplicitly]
    [LuaMember("getShapeAt")]
    public ILuaTileShape? GetShapeAt(int x, int y)
    {
        var position = new GridPosition(x, y);
        var tileData = _content.GetValueOrDefault(position);

        if (tileData == null)
        {
            return null;
        }

        if (tileData.TileType == TileType.Sprite)
        {
            return new LuaSpriteTileShape(tileData.SheetName ?? "", tileData.Frame, tileData.Angle, tileData.FlipX,
                tileData.FlipY);
        }

        if (tileData.TileType == TileType.Character)
        {
            return new LuaCharacterTileShape(tileData.TextString ?? "?");
        }

        return null;
    }

    [UsedImplicitly]
    [LuaMember("width")]
    public int Width()
    {
        return _width;
    }

    [UsedImplicitly]
    [LuaMember("height")]
    public int Height()
    {
        return _height;
    }

    [UsedImplicitly]
    [LuaMember("allTagsNames")]
    public Table GetTagNames()
    {
        var table = _luaRuntime.NewTable();

        HashSet<string> seenTags = new();
        foreach (var item in _content.Values)
        {
            if (item.HasExtraData())
            {
                var tag = item.ExtraData;
                if (!seenTags.Contains(tag))
                {
                    table.Append(DynValue.NewString(tag));
                }

                seenTags.Add(tag);
            }
        }

        return table;
    }

    [UsedImplicitly]
    [LuaMember("queryTagPositions")]
    public Table QueryTagPositions(string targetTag)
    {
        var result = _luaRuntime.NewTable();
        foreach (var (position, item) in _content)
        {
            if (item.ExtraData == targetTag)
            {
                var positionTable = _luaRuntime.NewTable();
                positionTable.Set("x",DynValue.NewNumber(position.X));
                positionTable.Set("y",DynValue.NewNumber(position.Y));
                result.Append(DynValue.NewTable(positionTable));
            }
        }
        return result;
    }

    private IEnumerable<GridPosition> GetPositionsRaw()
    {
        foreach (var position in _content.Keys)
        {
            yield return position;
        }
    }
}
