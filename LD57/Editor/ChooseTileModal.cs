using System;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class ChooseTileModal : Popup
{
    public ChooseTileModal(GridRectangle corners) : base(corners)
    {
        AddInputListener(keyboard =>
        {
            if (keyboard.GetButton(Keys.Escape).WasPressed)
            {
                Close();
            }
        });

        var topLeftPadding = new GridPosition(1, 1);
        var maxWidth = corners.Width - 2 - topLeftPadding.X;
        var x = 0;
        var y = 0;
        foreach (var (sheetName, sheet) in LdResourceAssets.Instance.Sheets)
        {
            if (sheet == null)
            {
                continue;
            }
            
            for (var frame = 0; frame < sheet.FrameCount; frame++)
            {
                var gridPosition = new GridPosition(x, y) + topLeftPadding;
                var capturedFrame = frame;
                var shape = new CanvasTileShapeSprite(sheetName, capturedFrame);
                var button = new Button(gridPosition, () => ChooseTile(shape));
                button.SetTileStateGetter(() => shape.GetTileState());
                AddButton(button);
                HandleLineFeed(ref x, ref y, maxWidth);
            }
        }

        for (int i = 0; i < 255; i++)
        {
            var gridPosition = new GridPosition(x, y) + topLeftPadding;

            var character = (char) i;
            if (char.IsAscii(character) && !char.IsControl(character) && !char.IsWhiteSpace(character))
            {
                var shape = new CanvasTileShapeString(character.ToString());
                var button = new Button(gridPosition,()=> ChooseTile(shape));
                button.SetTileStateGetter(() => shape.GetTileState());
                AddButton(button);
                HandleLineFeed(ref x, ref y, maxWidth);
            }
        }
    }

    private static void HandleLineFeed(ref int x, ref int y, int maxWidth)
    {
        x++;

        if (x > maxWidth)
        {
            x = 0;
            y++;
        }
    }

    private void ChooseTile(ICanvasTileShape shape)
    {
        OnChosen?.Invoke(shape);
        Close();
    }

    public event Action<ICanvasTileShape>? OnChosen;
}