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
                var button = new Button(gridPosition, () => ChooseTile(new CanvasTileShape(sheetName, capturedFrame)));
                button.SetTileStateGetter(() => TileState.Sprite(sheet, capturedFrame));
                AddButton(button);
                x++;

                if (x > maxWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }

    private void ChooseTile(ICanvasTileShape shape)
    {
        OnChosen?.Invoke(shape);
        Close();
    }

    public event Action<ICanvasTileShape>? OnChosen;
}