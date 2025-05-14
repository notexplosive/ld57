using System;
using ExplogineCore.Data;
using LD57.Core;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasBrushFilter : IBrushFilter
{
    private string _backgroundColorName = "white";
    private float _backgroundIntensity;
    private ICanvasTileShape _currentShape = new CanvasTileShapeSprite("Walls", 0);
    private XyBool _flipState;
    private string _foregroundColorName = "white";
    private QuarterRotation _rotation = QuarterRotation.Upright;
    public CanvasBrushLayer ForegroundShapeAndTransform { get; } = new(true, true);
    public CanvasBrushLayer ForegroundColor { get; } = new(true, true);
    public CanvasBrushLayer BackgroundColorAndIntensity { get; } = new(true, true);

    public CanvasTileData GetFullTile()
    {
        return CanvasTileData.FromSettings(_currentShape, _flipState, _rotation, _foregroundColorName,
            _backgroundColorName, _backgroundIntensity);
    }
    
    public UiElement CreateUi(AsciiScreen screen)
    {
        var panelSize = new GridPosition(4, 4);
        var topLeft = screen.RoomRectangle.TopRight + new GridPosition(-panelSize.X, 0);
        var element = new UiElement(new GridRectangle(topLeft, topLeft + panelSize));

        element.AddButton(new Button(new GridPosition(1, 1), OpenShapeModal)
            .SetTileStateGetter(GetForegroundShapeTileState)
            .SetTileStateOnHoverGetter(() => GetForegroundShapeTileState().WithForeground(Color.LimeGreen))
        );
        element.AddButton(
            new Button(new GridPosition(1, 2), OpenForegroundColorModal)
                .SetTileStateGetter(GetForegroundColorTileState)
                .SetTileStateOnHoverGetter(() => GetForegroundColorTileState().WithSprite(ResourceAlias.Tools, 0))
        );
        element.AddButton(
            new Button(new GridPosition(1, 3), OpenBackgroundColorModal)
                .SetTileStateGetter(GetBackgroundTileState)
                .SetTileStateOnHoverGetter(() => GetBackgroundTileState().WithSprite(ResourceAlias.Tools, 0))
        );

        CreateToggle(element, new GridPosition(2, 1), GetVisibleTileState(() => ForegroundShapeAndTransform.IsVisible),
            ForegroundShapeAndTransform.ToggleVisible);
        CreateToggle(element, new GridPosition(2, 2), GetVisibleTileState(() => ForegroundColor.IsVisible),
            ForegroundColor.ToggleVisible);
        CreateToggle(element, new GridPosition(2, 3), GetVisibleTileState(() => BackgroundColorAndIntensity.IsVisible),
            BackgroundColorAndIntensity.ToggleVisible);

        CreateToggle(element, new GridPosition(3, 1), GetEditingTileState(
                () => ForegroundShapeAndTransform.IsEditing, () => ForegroundShapeAndTransform.IsVisible),
            ForegroundShapeAndTransform.ToggleEditing);
        CreateToggle(element, new GridPosition(3, 2), GetEditingTileState(
                () => ForegroundColor.IsEditing, () => ForegroundColor.IsVisible),
            ForegroundColor.ToggleEditing);
        CreateToggle(element, new GridPosition(3, 3), GetEditingTileState(
                () => BackgroundColorAndIntensity.IsEditing, () => BackgroundColorAndIntensity.IsVisible),
            BackgroundColorAndIntensity.ToggleEditing);

        return element;
    }

    private static void CreateToggle(UiElement element, GridPosition position, Func<TileState> getVisibleTileState,
        Action doToggle)
    {
        element.AddButton(
            new Button(position, doToggle)
                .SetTileStateGetter(getVisibleTileState)
                .SetTileStateOnHoverGetter(() => getVisibleTileState().WithBackground(Color.LightBlue))
        );
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
        return GetFullTile().GetTile();
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

    private Func<TileState> GetEditingTileState(Func<bool> isEditable, Func<bool> isVisible)
    {
        return () =>
        {
            var result= GetEditableTileState(isEditable());

            if (!isVisible())
            {
                return result with {ForegroundColor = Color.Gray};
            }
            
            return result;
        };
    }

    private static TileState GetEditableTileState(bool editable)
    {
        if (editable)
        {
            return TileState.Sprite(ResourceAlias.Tools, 0, Color.White);
        }

        return TileState.Sprite(ResourceAlias.Tools, 9, Color.White);
    }

    private TileState GetBackgroundTileState()
    {
        return TileState.BackgroundOnly(ResourceAlias.Color(_backgroundColorName), _backgroundIntensity);
    }

    private TileState GetForegroundColorTileState()
    {
        return TileState.BackgroundOnly(ResourceAlias.Color(_foregroundColorName), 1f);
    }

    private TileState GetForegroundShapeTileState()
    {
        return _currentShape.GetTileState() with {Flip = _flipState, Angle = _rotation.Radians};
    }

    public CanvasTileData Combine(CanvasTileData currentTile, CanvasTileData incomingTile)
    {
        var result = incomingTile;

        if (!ForegroundShapeAndTransform.IsFunctionallyActive)
        {
            result = result.WithShapeData(currentTile.TileType, currentTile.SheetName, currentTile.Frame,
                currentTile.TextString, currentTile.FlipX, currentTile.FlipY, currentTile.Angle);
        }

        if (!ForegroundColor.IsFunctionallyActive)
        {
            result = result with
            {
                ForegroundColorName = currentTile.ForegroundColorName
            };
        }

        if (!BackgroundColorAndIntensity.IsFunctionallyActive)
        {
            result = result with
            {
                BackgroundIntensity = currentTile.BackgroundIntensity,
                BackgroundColorName = currentTile.BackgroundColorName
            };
        }

        return result;
    }
}
