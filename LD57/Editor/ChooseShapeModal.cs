using System;
using System.Collections.Generic;
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
    private readonly List<(ICanvasTileShape, GridPosition)> _shapesInOrder = new();

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

        AddExtraDraw(new ExtraDraw(DrawExtraBorder));

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
        mirrorHorizontallyButton.SetTileStateGetter(GetMirrorHorizontallyTile(getFlipState, _getRotation));
        AddButton(mirrorHorizontallyButton);

        var mirrorVerticallyButton = new Button(new GridPosition(3, 1),
            () => ChooseMirrorState(new XyBool(getFlipState().X, !getFlipState().Y)));
        mirrorVerticallyButton.SetTileStateGetter(GetMirrorVerticallyTile(getFlipState, _getRotation));
        AddButton(mirrorVerticallyButton);

        var rotateCcwButton = new Button(new GridPosition(4, 1),
            () => ChooseRotation(getRotation().CounterClockwisePrevious()));
        rotateCcwButton.SetTileStateGetter(GetCcwRotationTile);
        AddButton(rotateCcwButton);

        var rotateCwButton = new Button(new GridPosition(5, 1), () => ChooseRotation(getRotation().ClockwiseNext()));
        rotateCwButton.SetTileStateGetter(GetCwRotationTile);
        AddButton(rotateCwButton);

        var scrollAreaTopLeft = new GridPosition(1, 2);

        var scrollAreaBottomRight = new GridPosition(corners.Width - scrollAreaTopLeft.X - 1, corners.Height - 1);
        var pane = AddScrollablePane(
            new GridRectangle(scrollAreaTopLeft, scrollAreaBottomRight));

        var scrollBarRectangle = new GridRectangle(new GridPosition(scrollAreaBottomRight.X + 1, scrollAreaTopLeft.Y),
            scrollAreaBottomRight + new GridPosition(1, 0));
        var dynamicImage = AddDynamicImage(scrollBarRectangle);
        dynamicImage.SetDrawAction(screen =>
        {
            if (pane.ShouldHaveThumb())
            {
                var thumbPosition = pane.ThumbPosition(pane.ViewRectangle.Height);
                screen.PutFilledRectangle(TileState.Sprite(ResourceAlias.Tools, 17, Color.Gray),
                    scrollBarRectangle.MovedToZero());
                screen.PutTile(new GridPosition(0, thumbPosition),
                    TileState.Sprite(ResourceAlias.Tools, 16, Color.LightBlue));
            }
            else
            {
                screen.PutFilledRectangle(TileState.Sprite(ResourceAlias.Tools, 17, ResourceAlias.Color("background")),
                    scrollBarRectangle.MovedToZero());
            }
        });

        var maxWidth = pane.Rectangle.Width;
        var x = 0;
        var y = 0;
        var yToScrollTo = 0;

        var emptyShape = new CanvasTileShapeEmpty();
        var emptyButton = new Button(new GridPosition(x, y), () => ChooseTileAndClose(emptyShape));
        emptyButton.SetTileStateOnHoverGetter(() => GetHoveredTileState(emptyShape));
        pane.AddButton(emptyButton);
        HandleLineFeed(ref x, ref y, maxWidth);

        foreach (var (sheetName, sheet) in LdResourceAssets.Instance.Sheets)
        {
            if (sheet == null)
            {
                continue;
            }

            // todo: temporary hack to prevent the whole popupframe sprite from showing up
            if (sheetName == "PopupFrame")
            {
                continue;
            }

            for (var frame = 0; frame < sheet.FrameCount; frame++)
            {
                var gridPosition = new GridPosition(x, y);
                var capturedFrame = frame;
                var shape = new CanvasTileShapeSprite(sheetName, capturedFrame);
                var button = new Button(gridPosition, () => ChooseTileAndClose(shape));

                if (_getChosenShape().GetHashCode() == shape.GetHashCode())
                {
                    yToScrollTo = y;
                }

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

                _shapesInOrder.Add((shape, gridPosition));

                HandleLineFeed(ref x, ref y, maxWidth);
            }
        }

        pane.SetContentHeight(y);

        pane.ScrollToPosition(yToScrollTo);

        AddScrollListener(modifierKeys => modifierKeys.Alt, delta =>
        {
            if (delta == 0)
            {
                return;
            }
            var currentIndex = _shapesInOrder.FindIndex(a => a.Item1.GetHashCode() == _getChosenShape().GetHashCode());
            var newIndex = currentIndex + delta;

            if (_shapesInOrder.IsValidIndex(newIndex))
            {
                ChooseTile(_shapesInOrder[newIndex].Item1);
                if (!pane.IsInView(_shapesInOrder[newIndex].Item2.Y))
                {
                    pane.ScrollToPosition(_shapesInOrder[newIndex].Item2.Y);
                }
            }
        });
    }

    public static Func<TileState> GetMirrorHorizontallyTile(Func<XyBool> getFlipState, Func<QuarterRotation> rotation)
    {
        return () =>
        {
            var frame = 10;
            if (getFlipState().X)
            {
                frame = 11;
            }

            return TileState.Sprite(ResourceAlias.Tools, frame) with
            {
                ForegroundColor = Color.LightBlue, Angle = rotation().Radians
            };
        };
    }

    public static Func<TileState> GetMirrorVerticallyTile(Func<XyBool> getFlipState, Func<QuarterRotation> rotation)
    {
        return () =>
        {
            var frame = 12;
            if (getFlipState().Y)
            {
                frame = 13;
            }

            return TileState.Sprite(ResourceAlias.Tools, frame) with
            {
                ForegroundColor = Color.LightBlue, Angle = rotation().Radians
            };
        };
    }

    public static TileState GetCcwRotationTile()
    {
        return TileState.Sprite(ResourceAlias.Entities, 27) with {ForegroundColor = Color.LightBlue};
    }

    public static TileState GetCwRotationTile()
    {
        return TileState.Sprite(ResourceAlias.Entities, 27) with
        {
            Flip = new XyBool(true, false), ForegroundColor = Color.LightBlue
        };
    }

    private void DrawExtraBorder(AsciiScreen screen)
    {
        var width = Rectangle.Width;
        screen.PutTile(new GridPosition(width, 0), TileState.Sprite(ResourceAlias.PopupFrame, 1));
        screen.PutTile(new GridPosition(width, 1), TileState.TransparentEmpty);
        screen.PutTile(new GridPosition(width, 2),
            TileState.Sprite(ResourceAlias.Utility, 27).WithFlip(new XyBool(true, false)));
        screen.PutTile(new GridPosition(width + 1, 2), TileState.Sprite(ResourceAlias.PopupFrame, 5));
        screen.PutTile(new GridPosition(width + 1, 0), TileState.Sprite(ResourceAlias.PopupFrame, 1));
        screen.PutTile(new GridPosition(width + 2, 0), TileState.Sprite(ResourceAlias.PopupFrame, 2));
        screen.PutTile(new GridPosition(width + 2, 1), TileState.Sprite(ResourceAlias.PopupFrame, 3));
        screen.PutTile(new GridPosition(width + 2, 2), TileState.Sprite(ResourceAlias.PopupFrame, 4));
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

    private void ChooseTileAndClose(ICanvasTileShape shape)
    {
        ChooseTile(shape);
        Close();
    }

    private void ChooseTile(ICanvasTileShape shape)
    {
        ChoseShape?.Invoke(shape);
    }

    public event Action<ICanvasTileShape>? ChoseShape;

    protected override void OnClickedNothing()
    {
        Close();
    }

    public static Func<TileState> GetCurrentRotationTile(Func<QuarterRotation> getRotation)
    {
        return () => TileState.Sprite(ResourceAlias.Tools, 22).WithRotation(getRotation()) with
        {
            ForegroundColor = getRotation() != QuarterRotation.Upright ? Color.Yellow : Color.LightBlue
        };
    }
}
