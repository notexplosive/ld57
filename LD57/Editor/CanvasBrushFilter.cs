using System;
using ExplogineCore.Data;
using LD57.CartridgeManagement;
using LD57.Core;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasBrushFilter : IBrushFilter
{
    private string _backgroundColorName = "white";
    private float _backgroundIntensity;
    private ICanvasTileShape _currentShape = new CanvasTileShapeSprite("Walls", 0);
    private string _foregroundColorName = "white";
    public XyBool FlipState { get; set; }
    public QuarterRotation Rotation { get; set; } = QuarterRotation.Upright;

    public CanvasBrushLayer ForegroundShapeAndTransform { get; } = new(true, true);
    public CanvasBrushLayer ForegroundColor { get; } = new(true, true);
    public CanvasBrushLayer BackgroundColorAndIntensity { get; } = new(true, true);

    private static GridPosition PanelSize => new(4, 4);

    public CanvasTileData GetFullTile()
    {
        return CanvasTileData.FromSettings(_currentShape, FlipState, Rotation, _foregroundColorName,
            _backgroundColorName, _backgroundIntensity);
    }

    public UiElement CreateUi(AsciiScreen screen)
    {
        var topLeft = PanelTopLeft(screen.RoomRectangle);
        var element = new UiElement(new GridRectangle(topLeft, topLeft + PanelSize));

        element.AddButton(new Button(new GridPosition(1, 1), () => OpenShapeModal(screen.RoomRectangle))
            .SetTileStateGetter(GetForegroundShapeTileState)
            .SetTileStateOnHoverGetter(() => GetForegroundShapeTileState().WithForeground(Color.LimeGreen))
        );
        element.AddButton(
            new Button(new GridPosition(1, 2), () => OpenForegroundColorModal(screen.RoomRectangle))
                .SetTileStateGetter(GetForegroundColorTileState)
                .SetTileStateOnHoverGetter(() => GetForegroundColorTileState().WithSprite(ResourceAlias.Tools, 0))
        );
        element.AddButton(
            new Button(new GridPosition(1, 3), () => OpenBackgroundColorModal(screen.RoomRectangle))
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

    private static GridPosition PanelTopLeft(GridRectangle screenSize)
    {
        return screenSize.TopRight + new GridPosition(-PanelSize.X, 0);
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

    public void OpenForegroundColorModal(GridRectangle screenSize)
    {
        var modal = new ChooseColorModal(
            new GridRectangle(PanelTopLeft(screenSize) + new GridPosition(-ColorModalWidth(), 10), PanelTopLeft(screenSize))
                .Moved(new GridPosition(0, 1)),
            () => _foregroundColorName);

        modal.ChoseColor += SetForegroundColor;
        RequestModal(modal);
    }

    private static int ColorModalWidth()
    {
        var width = 1;
        foreach (var color in LdResourceAssets.Instance.NamedColors)
        {
            width = Math.Max(color.Key.Length, width);
        }

        return width + 3;
    }

    public void OpenBackgroundColorModal(GridRectangle screenSize)
    {
        var modal = new ChooseColorModal(
            new GridRectangle(PanelTopLeft(screenSize) + new GridPosition(-ColorModalWidth(), 10), PanelTopLeft(screenSize))
                .Moved(new GridPosition(0, 2)),
            () => _backgroundColorName, () => _backgroundIntensity);

        modal.ChoseColor += SetBackgroundColor;
        modal.ChoseIntensity += SetIntensity;
        RequestModal(modal);
    }

    private void RequestModal(Popup modal)
    {
        RequestedModal?.Invoke(modal);
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

    public void OpenShapeModal(GridRectangle screenRectangle)
    {
        var chooseTileModal = new ChooseShapeModal(
            new GridRectangle(PanelTopLeft(screenRectangle), PanelTopLeft(screenRectangle) + new GridPosition(-15, 10)),
            () => _currentShape, () => FlipState, () => Rotation);
        chooseTileModal.ChoseShape += SetTileShape;
        chooseTileModal.ChoseFlipState += SetFlipState;
        chooseTileModal.ChoseRotation += ChooseRotation;
        RequestModal(chooseTileModal);
    }

    private void ChooseRotation(QuarterRotation rotation)
    {
        Rotation = rotation;
    }

    private void SetFlipState(XyBool mirrorState)
    {
        FlipState = mirrorState;
    }

    public void SetTileShape(ICanvasTileShape shape)
    {
        _currentShape = shape;
    }

    public event Action<Popup>? RequestedModal;

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
            var result = GetEditableTileState(isEditable());

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
        return _currentShape.GetTileState() with {Flip = FlipState, Angle = Rotation.Radians};
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

    /// <summary>
    ///     Gets the current Shape and Colors based on what filters are currently active
    /// </summary>
    public void SetBasedOnData(CanvasTileData tileData)
    {
        if (ForegroundShapeAndTransform.IsFunctionallyActive)
        {
            _currentShape = tileData.GetShape();
            Rotation = QuarterRotation.FromAngle(tileData.Angle);
            FlipState = new XyBool(tileData.FlipX, tileData.FlipY);
        }

        if (ForegroundColor.IsFunctionallyActive)
        {
            _foregroundColorName = tileData.ForegroundColorName ?? "white";
        }

        if (BackgroundColorAndIntensity.IsFunctionallyActive)
        {
            _backgroundIntensity = tileData.BackgroundIntensity;
            _backgroundColorName = tileData.BackgroundColorName ?? "white";
        }
    }
}
