using System;
using System.IO;
using ExplogineCore;
using ExplogineMonoGame;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace LD57.Editor;

public class WorldEditorSurface
{
    private readonly EditorSession _editorSession;

    public WorldEditorSurface(EditorSession editorSession)
    {
        _editorSession = editorSession;
    }

    public WorldSelection WorldSelection { get; } = new();

    public string? FileName { get; set; }

    public WorldTemplate WorldTemplate { get; private set; } = new();

    public void HandleKeyBinds(ConsumableInput input)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.F5).WasPressed)
            {
                var player = WorldTemplate.GetPlayerEntity();
                var position = new GridPosition();
                if (player != null)
                {
                    position = player.Position;
                }

                _editorSession.RequestPlayAt(position);
            }
        }
    }

    public void PaintWorldToScreen(AsciiScreen screen, GridPosition cameraPosition, float dt)
    {
        var world = new World(Constants.GameRoomSize, WorldTemplate, true);

        var player = WorldTemplate.GetPlayerEntity();
        if (player != null)
        {
            world.AddEntity(new Entity(world, player.Position,
                ResourceAlias.EntityTemplate("player") ?? new EntityTemplate()));
        }

        world.SetCurrentRoom(new Room(world, cameraPosition, cameraPosition + screen.RoomSize));
        world.CameraPosition = cameraPosition;

        world.PaintToScreen(screen, dt);
    }

    public void PaintOverlayBelowTool(AsciiScreen screen, GridPosition cameraPosition)
    {
        // create empty room so we don't have to 
        var world = new World(Constants.GameRoomSize, new WorldTemplate());

        if (_editorSession.HoveredWorldPosition.HasValue)
        {
            var hoveredRoom = world.GetRoomAt(_editorSession.HoveredWorldPosition.Value);
            var hoveredRoomTopLeft = hoveredRoom.TopLeft - cameraPosition;
            var hoveredRoomBottomRight = hoveredRoom.BottomRight - cameraPosition;

            var width = hoveredRoomBottomRight.X - hoveredRoomTopLeft.X;
            var height = hoveredRoomBottomRight.Y - hoveredRoomTopLeft.Y;

            foreach (var position in Constants.TraceRectangle(hoveredRoomTopLeft, hoveredRoomBottomRight))
            {
                var color = Color.LightBlue;
                var previousTileState = screen.GetTile(position);
                var increment = 2;

                var normalX = position.X - hoveredRoomTopLeft.X;
                var normalY = position.Y - hoveredRoomTopLeft.Y;

                if (normalX % increment == 0 && (normalY == 0 || normalY == height))
                {
                    color = Color.Lime;
                }

                if (normalY % increment == 0 && (normalX == 0 || normalX == width))
                {
                    color = Color.ForestGreen;
                }

                var midX = hoveredRoomTopLeft.X + width / 2;
                if (position.X == midX || position.X == midX + 1)
                {
                    color = Color.Orange;
                }

                var midY = hoveredRoomTopLeft.Y + height / 2;

                if (position.Y == midY || position.Y == midY + 1)
                {
                    color = Color.Orange;
                }

                screen.PutTile(position, previousTileState with {BackgroundColor = color, BackgroundIntensity = 1f});
            }
        }
        
        
        foreach (var worldPosition in WorldSelection.AllPositions())
        {
            var screenPosition = worldPosition - cameraPosition;
            if (screen.ContainsPosition(screenPosition))
            {
                screen.PutTile(screenPosition, WorldSelection.GetTileState(worldPosition - WorldSelection.Offset));
            }
        }
    }

    public void PaintOverlayAboveTool(AsciiScreen screen, GridPosition cameraPosition)
    {
        if (MathF.Sin(Client.TotalElapsedTime * 10) > 0)
        {
            foreach (var placedEntity in WorldTemplate.PlacedEntities)
            {
                if (placedEntity.ExtraState.ContainsKey(Constants.CommandKey))
                {
                    screen.PutTile(placedEntity.Position - cameraPosition,
                        TileState.StringCharacter("!", Color.OrangeRed));
                }
            }
        }
    }

    public void Save(string fileName)
    {
        Client.Debug.RepoFileSystem.WriteToFile($"Resource/Worlds/{fileName}.json",
            JsonConvert.SerializeObject(WorldTemplate, Formatting.Indented));
    }

    public void Open(string path, bool isFullPath)
    {
        var fileName = path;
        WorldTemplate? worldTemplate;

        if (isFullPath)
        {
            fileName = new FileInfo(path).Name;
            worldTemplate = Constants.AttemptLoadWorldTemplateFromFullPath(path);
        }
        else
        {
            worldTemplate = Constants.AttemptLoadWorldTemplateFromWorldDirectory(path);
        }

        if (worldTemplate != null)
        {
            var newFileName = fileName.RemoveFileExtension();
            SetTemplate(newFileName, worldTemplate);
        }
    }

    public void Clear()
    {
        SetTemplate(null, new WorldTemplate());
    }

    private void SetTemplate(string? newFileName, WorldTemplate newWorld)
    {
        FileName = newFileName;
        WorldTemplate = newWorld;
        _editorSession.ResetCameraPosition();
    }
}
