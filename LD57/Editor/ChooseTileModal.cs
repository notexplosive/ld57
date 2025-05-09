using System;
using ExplogineCore.Data;
using LD57.CartridgeManagement;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class ChooseTileModal : Popup
{
    private readonly Func<ICanvasTileShape> _getChosenShape;
    private readonly Func<XyBool> _getFlipState;

    public ChooseTileModal(GridRectangle corners, Func<ICanvasTileShape> getChosenShape, Func<XyBool> getFlipState) : base(corners)
    {
        _getChosenShape = getChosenShape;
        _getFlipState = getFlipState;
        AddInputListener(keyboard =>
        {
            if (keyboard.GetButton(Keys.Escape).WasPressed)
            {
                Close();
            }
        });
        
        var mirrorHorizontallyButton = new Button(new GridPosition(1,1), () => ChooseMirrorState(new XyBool(X: !getFlipState().X, Y: getFlipState().Y)));
        mirrorHorizontallyButton.SetTileStateGetter(() => TileState.Sprite(ResourceAlias.Tools, 10));
        AddButton(mirrorHorizontallyButton);
        var mirrorVerticallyButton = new Button(new GridPosition(2,1), () => ChooseMirrorState(new XyBool(X: getFlipState().X, Y: !getFlipState().Y)));
        mirrorVerticallyButton.SetTileStateGetter(() => TileState.Sprite(ResourceAlias.Tools, 12));
        AddButton(mirrorVerticallyButton);

        var topLeftPadding = new GridPosition(1, 2);
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
                button.SetTileStateGetter(() =>
                {
                    if (_getChosenShape().GetHashCode() == shape.GetHashCode())
                    {
                        return GetHighlightedTileState(shape);
                    }

                    return shape.GetTileState() with {Flip = getFlipState()};
                });
                AddButton(button);
                HandleLineFeed(ref x, ref y, maxWidth);
            }
        }

        for (var i = 0; i < 255; i++)
        {
            var gridPosition = new GridPosition(x, y) + topLeftPadding;

            var character = (char) i;
            if (char.IsAscii(character) && !char.IsControl(character) && !char.IsWhiteSpace(character))
            {
                var shape = new CanvasTileShapeString(character.ToString());
                var button = new Button(gridPosition, () => ChooseTile(shape));
                button.SetTileStateGetter(() =>
                {
                    if (_getChosenShape().GetHashCode() == shape.GetHashCode())
                    {
                        return GetHighlightedTileState(shape);
                    }

                    return shape.GetTileState() with {Flip = getFlipState()};
                });
                AddButton(button);
                HandleLineFeed(ref x, ref y, maxWidth);
            }
        }
    }

    private void ChooseMirrorState(XyBool xyBool)
    {
        ChoseFlipState?.Invoke(xyBool);
    }

    public event Action<XyBool>? ChoseFlipState;

    private TileState GetHighlightedTileState(ICanvasTileShape shape)
    {
        return shape.GetTileState() with {ForegroundColor = Color.Orange, Flip = _getFlipState()};
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
        ChoseShape?.Invoke(shape);
        Close();
    }

    public event Action<ICanvasTileShape>? ChoseShape;
}
