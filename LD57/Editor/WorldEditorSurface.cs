using System;
using System.IO;
using ExplogineCore;
using ExplogineMonoGame;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class WorldEditorSurface : EditorSurface<WorldTemplate, PlacedEntity, EntityTemplate>
{
    private readonly WorldSelection _selection;

    public WorldEditorSurface() : base("Worlds")
    {
        _selection = new WorldSelection(this);
    }

    protected override WorldSelection RealSelection => _selection;

    public override void HandleKeyBinds(ConsumableInput input)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.F5).WasPressed)
            {
                var player = Data.GetPlayerEntity();
                var position = new GridPosition();
                if (player != null)
                {
                    position = player.Position;
                }

                RequestedPlayAt?.Invoke(position);
            }
        }
    }

    public override void PaintWorldToScreen(AsciiScreen screen, float dt)
    {
        var world = new World(Constants.GameRoomSize, Data, true);

        var player = Data.GetPlayerEntity();
        if (player != null)
        {
            world.AddEntity(new Entity(world, player.Position,
                ResourceAlias.EntityTemplate("player") ?? new EntityTemplate()));
        }

        // world.SetCurrentRoom(new Room(world, _cameraPosition, _cameraPosition + screen.RoomSize));
        // world.CameraPosition = _cameraPosition;

        world.PaintToScreen(screen, dt);
    }

    public override void PaintOverlayBelowTool(AsciiScreen screen, GridPosition? hoveredPosition)
    {
        // create empty room so we don't have to 
        var world = new World(Constants.GameRoomSize, new WorldTemplate());

        if (hoveredPosition.HasValue)
        {
            var hoveredRoom = world.GetRoomAt(hoveredPosition.Value);
            var hoveredRoomTopLeft = hoveredRoom.TopLeft;
            var hoveredRoomBottomRight = hoveredRoom.BottomRight;

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
    }

    public override void PaintOverlayAboveTool(AsciiScreen screen)
    {
        if (MathF.Sin(Client.TotalElapsedTime * 10) > 0)
        {
            foreach (var placedEntity in Data.Content)
            {
                if (placedEntity.ExtraState.ContainsKey(Constants.CommandKey))
                {
                    screen.PutTile(placedEntity.Position, TileState.StringCharacter("!", Color.OrangeRed));
                }
            }
        }
    }

    public override void Open(string path, bool isFullPath)
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
            SetTemplateAndFileName(newFileName, worldTemplate);
        }
    }

    public event Action<GridPosition>? RequestedPlayAt;

    public void RequestPlay(GridPosition position)
    {
        RequestedPlayAt?.Invoke(position);
    }
}
