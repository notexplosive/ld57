using System;
using ExplogineCore.Data;
using LD57.Core;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasBrushMode
{
    private string _backgroundColorName = "white";
    private float _backgroundIntensity;
    private ICanvasTileShape _currentShape = new CanvasTileShapeSprite("Walls", 0);
    private XyBool _flipState;
    private string _foregroundColorName = "white";
    private QuarterRotation _rotation = QuarterRotation.Upright;
    public CanvasBrushLayer ForegroundShapeAndTransform { get; set; } = new(true, true);
    public CanvasBrushLayer ForegroundColor { get; set; } = new(true, true);
    public CanvasBrushLayer BackgroundColorAndIntensity { get; set; } = new(true, true);

    public CanvasTileData GetTile()
    {
        return CanvasTileData.FromSettings(_currentShape, _flipState, _rotation, _foregroundColorName,
            _backgroundColorName, _backgroundIntensity);
    }

    public UiElement CreateUi(AsciiScreen screen)
    {
        var panelSize = new GridPosition(4, 5);
        var topLeft = screen.Rectangle.TopRight + new GridPosition(-panelSize.X, 0);
        var element = new UiElement(new GridRectangle(topLeft, topLeft + panelSize + new GridPosition(1, 1)));

        element.AddButton(
            new Button(new GridPosition(1, 1), OpenShapeModal)
                .SetTileStateGetter(GetForegroundShape));
        element.AddButton(
            new Button(new GridPosition(1, 2), OpenForegroundColorModal)
                .SetTileStateGetter(GetForegroundColorTileState));
        element.AddButton(
            new Button(new GridPosition(1, 3), OpenBackgroundColorModal)
                .SetTileStateGetter(GetBackgroundTileState));

        element.AddDynamicTile(new GridPosition(2, 1),
            GetVisibleTileState(() => ForegroundShapeAndTransform.IsVisible));
        element.AddDynamicTile(new GridPosition(2, 2), GetVisibleTileState(() => ForegroundColor.IsVisible));
        element.AddDynamicTile(new GridPosition(2, 3),
            GetVisibleTileState(() => BackgroundColorAndIntensity.IsVisible));

        element.AddDynamicTile(new GridPosition(3, 1),
            GetEditingTileState(() => ForegroundShapeAndTransform.IsEditing));
        element.AddDynamicTile(new GridPosition(3, 2), GetEditingTileState(() => ForegroundColor.IsEditing));
        element.AddDynamicTile(new GridPosition(3, 3),
            GetEditingTileState(() => BackgroundColorAndIntensity.IsEditing));

        return element;
    }

    private void OpenForegroundColorModal()
    {
        var modal = new ChooseColorModal(new GridRectangle(new GridPosition(5, 5), new GridPosition(20, 20)),
            () => _foregroundColorName);

        modal.ChoseColor += SetForegroundColor;
        RequestModal(modal);
    }

    private void OpenBackgroundColorModal()
    {
        var modal = new ChooseColorModal(new GridRectangle(new GridPosition(5, 5), new GridPosition(20, 20)),
            () => _backgroundColorName, () => _backgroundIntensity);

        modal.ChoseColor += SetBackgroundColor;
        modal.ChoseIntensity += SetIntensity;
        RequestModal(modal);
    }

    private void RequestModal(Popup chooseTileModal)
    {
        RequestedModal?.Invoke(chooseTileModal);
    }

    private void SetIntensity(float intensity)
    {
        _backgroundIntensity = intensity;
    }

    private void SetBackgroundColor(string color)
    {
        _backgroundColorName = color;
    }

    private void SetForegroundColor(string color)
    {
        _foregroundColorName = color;
    }

    private void OpenShapeModal()
    {
        var chooseTileModal = new ChooseShapeModal(new GridRectangle(new GridPosition(5, 5), new GridPosition(20, 20)),
            () => _currentShape, () => _flipState, () => _rotation);
        chooseTileModal.ChoseShape += SetTileShape;
        chooseTileModal.ChoseFlipState += SetFlipState;
        chooseTileModal.ChoseRotation += ChooseRotation;
        RequestModal(chooseTileModal);
    }

    private void ChooseRotation(QuarterRotation rotation)
    {
        _rotation = rotation;
    }

    private void SetFlipState(XyBool mirrorState)
    {
        _flipState = mirrorState;
    }

    public void SetTileShape(ICanvasTileShape shape)
    {
        _currentShape = shape;
    }

    public event Action<Popup>? RequestedModal;

    private TileState GetCurrentTileState()
    {
        return GetTile().FullTileState();
    }

    private Func<TileState> GetVisibleTileState(Func<bool> getter)
    {
        return () =>
        {
            if (getter())
            {
                return TileState.Sprite(ResourceAlias.Tools, 7, Color.White);
            }

            return TileState.Sprite(ResourceAlias.Tools, 8, Color.White);
        };
    }

    private Func<TileState> GetEditingTileState(Func<bool> getter)
    {
        return () =>
        {
            if (getter())
            {
                return TileState.Sprite(ResourceAlias.Tools, 0, Color.White);
            }

            return TileState.Sprite(ResourceAlias.Tools, 9, Color.White);
        };
    }

    private TileState GetBackgroundTileState()
    {
        return TileState.BackgroundOnly(ResourceAlias.Color(_backgroundColorName), _backgroundIntensity);
    }

    private TileState GetForegroundColorTileState()
    {
        return TileState.BackgroundOnly(ResourceAlias.Color(_foregroundColorName), 1f);
    }

    private TileState GetForegroundShape()
    {
        return _currentShape.GetTileState() with {Flip = _flipState};
    }
}
