using System;
using ExplogineCore.Data;
using LD57.CartridgeManagement;
using LD57.Core;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class ChooseShapeModal : Popup
{
    private readonly Func<ICanvasTileShape> _getChosenShape;
    private readonly Func<XyBool> _getFlipState;
    private readonly Func<QuarterRotation> _getRotation;

    public ChooseShapeModal(GridRectangle corners, Func<ICanvasTileShape> getChosenShape, Func<XyBool> getFlipState,
        Func<QuarterRotation> getRotation) : base(corners)
    {
        _getChosenShape = getChosenShape;
        _getFlipState = getFlipState;
        _getRotation = getRotation;
        AddInputListener(keyboard =>
        {
            if (keyboard.GetButton(Keys.Escape).WasPressed)
            {
                Close();
            }
        });

        var resetButton = new Button(new GridPosition(1, 1), ResetTransformations);
        resetButton.SetTileStateGetter(() =>
        {
            var color = Color.Gray;

            if (_getFlipState().X || _getFlipState().Y || _getRotation() != QuarterRotation.Upright)
            {
                color = Color.Yellow;
            }

            return TileState.Sprite(ResourceAlias.Tools, 14) with {ForegroundColor = color};
        });
        AddButton(resetButton);

        var mirrorHorizontallyButton = new Button(new GridPosition(2, 1),
            () => ChooseMirrorState(new XyBool(!getFlipState().X, getFlipState().Y)));
        mirrorHorizontallyButton.SetTileStateGetter(() =>
        {
            var frame = 10;
            if (getFlipState().X)
            {
                frame = 11;
            }

            return TileState.Sprite(ResourceAlias.Tools, frame) with {ForegroundColor = Color.LightBlue};
        });
        AddButton(mirrorHorizontallyButton);

        var mirrorVerticallyButton = new Button(new GridPosition(3, 1),
            () => ChooseMirrorState(new XyBool(getFlipState().X, !getFlipState().Y)));
        mirrorVerticallyButton.SetTileStateGetter(() =>
        {
            var frame = 12;
            if (getFlipState().Y)
            {
                frame = 13;
            }

            return TileState.Sprite(ResourceAlias.Tools, frame) with {ForegroundColor = Color.LightBlue};
        });
        AddButton(mirrorVerticallyButton);

        var rotateCcwButton = new Button(new GridPosition(4, 1),
            () => ChooseRotation(getRotation().CounterClockwisePrevious()));
        rotateCcwButton.SetTileStateGetter(() =>
            TileState.Sprite(ResourceAlias.Entities, 27) with {ForegroundColor = Color.LightBlue});
        AddButton(rotateCcwButton);

        var rotateCwButton = new Button(new GridPosition(5, 1), () => ChooseRotation(getRotation().ClockwiseNext()));
        rotateCwButton.SetTileStateGetter(() => TileState.Sprite(ResourceAlias.Entities, 27) with
        {
            Flip = new XyBool(true, false), ForegroundColor = Color.LightBlue
        });
        AddButton(rotateCwButton);

        var topLeftPadding = new GridPosition(1, 2);

        var pane = AddScrollablePane(new GridRectangle(topLeftPadding,
            new GridPosition(corners.Width - topLeftPadding.X, corners.Height - 1)));

        var maxWidth = pane.Rectangle.Width;
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
                var gridPosition = new GridPosition(x, y);
                var capturedFrame = frame;
                var shape = new CanvasTileShapeSprite(sheetName, capturedFrame);
                var button = new Button(gridPosition, () => ChooseTile(shape));
                button.SetTileStateGetter(() =>
                {
                    if (_getChosenShape().GetHashCode() == shape.GetHashCode())
                    {
                        return GetHighlightedTileState(shape);
                    }

                    return GetBasicTileState(shape);
                });
                button.SetTileStateOnHoverGetter(() => GetHoveredTileState(shape));
                pane.AddButton(button);
                HandleLineFeed(ref x, ref y, maxWidth);
            }
        }

        for (var i = 0; i < 255; i++)
        {
            var gridPosition = new GridPosition(x, y);

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

                    return GetBasicTileState(shape);
                });
                button.SetTileStateOnHoverGetter(() => GetHoveredTileState(shape));
                pane.AddButton(button);
                HandleLineFeed(ref x, ref y, maxWidth);
            }
        }

        pane.SetContentHeight(y);
    }

    private void ResetTransformations()
    {
        ChooseRotation(QuarterRotation.Upright);
        ChooseMirrorState(XyBool.False);
    }

    private TileState GetHoveredTileState(ICanvasTileShape shape)
    {
        return GetBasicTileState(shape).WithBackground(Color.Red);
    }

    private TileState GetBasicTileState(ICanvasTileShape shape)
    {
        return shape.GetTileState() with {Flip = _getFlipState(), Angle = _getRotation().Radians};
    }

    private void ChooseRotation(QuarterRotation newRotation)
    {
        ChoseRotation?.Invoke(newRotation);
    }

    public event Action<QuarterRotation>? ChoseRotation;

    private void ChooseMirrorState(XyBool xyBool)
    {
        ChoseFlipState?.Invoke(xyBool);
    }

    public event Action<XyBool>? ChoseFlipState;

    private TileState GetHighlightedTileState(ICanvasTileShape shape)
    {
        return GetBasicTileState(shape) with {ForegroundColor = Color.Orange};
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
