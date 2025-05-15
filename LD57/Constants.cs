using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace LD57;

public static class Constants
{
    public const string CommandKey = "command";
    
    public static GridPosition GameRoomSize => CreateGameScreen().RoomSize - new GridPosition(0, 3);
    public static string Title => "Ipsum Painter";

    public static AsciiScreen CreateGameScreen()
    {
        return new AsciiScreen(40, 22, TileSize);
    }

    public static int TileSize => 48;

    public static Rectangle CreateRectangle(GridPosition topLeft, GridPosition bottomRight, bool isInclusive = true)
    {
        var extra = new GridPosition(1, 1);

        if (!isInclusive)
        {
            extra = new GridPosition(0, 0);
        }

        return new Rectangle(topLeft.ToPoint(),
            (bottomRight - topLeft + extra).ToPoint());
    }

    public static IEnumerable<GridPosition> AllPositionsInRectangle(GridPosition a, GridPosition b)
    {
        var minX = Math.Min(a.X, b.X);
        var minY = Math.Min(a.Y, b.Y);
        var width = Math.Abs(a.X - b.X);
        var height = Math.Abs(a.Y - b.Y);

        for (var x = 0; x < width+1; x++)
        {
            for (var y = 0; y < height+1; y++)
            {
                yield return new GridPosition(minX, minY) + new GridPosition(x, y);
            }
        }
    }

    public static IEnumerable<GridPosition> TraceRectangle(GridPosition a, GridPosition b)
    {
        var minX = Math.Min(a.X, b.X);
        var minY = Math.Min(a.Y, b.Y);
        var width = Math.Abs(a.X - b.X);
        var height = Math.Abs(a.Y - b.Y);

        var topLeft = new GridPosition(minX, minY);
        var bottomRight = new GridPosition(minX + width, minY + height);
        var bottomLeft = new GridPosition(minX, minY + height);
        var topRight = new GridPosition(minX + width, minY);

        yield return topLeft;
        for (var i = 1; i < width; i++)
        {
            yield return topLeft + new GridPosition(i, 0);
        }

        yield return topRight;

        for (var i = 1; i < height; i++)
        {
            yield return topRight + new GridPosition(0, i);
        }

        yield return bottomRight;

        for (var i = 1; i < width; i++)
        {
            yield return bottomLeft + new GridPosition(i, 0);
        }

        yield return bottomLeft;

        for (var i = 1; i < height; i++)
        {
            yield return topLeft + new GridPosition(0, i);
        }
    }

    public static int RoundToInt(float x)
    {
        return (int) Math.Round(x);
    }

    public static GridPosition RoundToGridPosition(this Vector2 vector2)
    {
        return new GridPosition(RoundToInt(vector2.X), RoundToInt(vector2.Y));
    }

    public static WorldTemplate? AttemptLoadWorldTemplateFromWorldDirectory(string worldNameWithoutExtension)
    {
        var worldData = Client.Debug.RepoFileSystem.GetDirectory("Resource/Worlds").ReadFile(worldNameWithoutExtension + ".json");
        return JsonConvert.DeserializeObject<WorldTemplate>(worldData);
    }

    public static void WriteJsonToResources<T>(T data, string subDirectory, string fileName)
    {
        Client.Debug.RepoFileSystem.WriteToFile($"Resource/{subDirectory}/{fileName}.json",
            JsonConvert.SerializeObject(data, Formatting.Indented));
    }
}
