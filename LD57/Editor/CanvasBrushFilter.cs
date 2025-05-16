using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using LD57.CartridgeManagement;
using LD57.Core;
using LD57.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD57.Editor;

public class CanvasBrushFilter : IBrushFilter
{
    private readonly List<KeybindChord> _chords;
    private string _backgroundColorName = "white";
    private float _backgroundIntensity;
    private ICanvasTileShape _currentShape = new CanvasTileShapeSprite("Walls", 0);
    private XyBool _flipState;
    private string _foregroundColorName = "white";
    private QuarterRotation _rotation = QuarterRotation.Upright;

    public CanvasBrushFilter(List<KeybindChord> chords)
    {
        _chords = chords;
    }

    public CanvasBrushLayer ForegroundShapeAndTransform { get; } = new(true, true);
    public CanvasBrushLayer ForegroundColor { get; } = new(true, true);
    public CanvasBrushLayer BackgroundColorAndIntensity { get; } = new(true, true);

    private static GridPosition PanelSize => new(4, 4);

    public CanvasTileData GetFullTile()
    {
        return CanvasTileData.FromSettings(_currentShape, _flipState, _rotation, _foregroundColorName,
            _backgroundColorName, _backgroundIntensity);
    }

    public UiElement CreateUi(AsciiScreen screen)
    {
        var topLeft = PanelTopLeft(screen);
        var element = new UiElement(new GridRectangle(topLeft, topLeft + PanelSize));

        _chords.Add(new KeybindChord(Keys.E, "Brush Filter")
            .Add(Keys.S, "Shape", true,() => OpenShapeModal(screen))
            .Add(Keys.C, "Foreground Color", true,() => OpenForegroundColorModal(screen))
            .Add(Keys.F, "Background Color", true,() => OpenBackgroundColorModal(screen))
        );

        _chords.Add(new KeybindChord(Keys.T, "Transform")
            .AddDynamicTile(ChooseShapeModal.GetMirrorHorizontallyTile(()=>_flipState, ()=>_rotation))
            .AddDynamicTile(ChooseShapeModal.GetMirrorVerticallyTile(()=>_flipState, ()=>_rotation))
            .AddDynamicTile(ChooseShapeModal.GetCurrentRotationTile(()=>_rotation))
            .Add(Keys.R, "Reset Transform", false,() =>
            {
                _rotation = QuarterRotation.Upright;
                _flipState = XyBool.False;
            })
            .Add(Keys.H, "Flip Horizontal", false,() =>
            {
                if (_rotation == QuarterRotation.Upright || _rotation == QuarterRotation.Half)
                {
                    _flipState.X = !_flipState.X;
                }
                else
                {
                    _flipState.Y = !_flipState.Y;
                }
            })
            .Add(Keys.V, "Flip Vertical", false,() =>
            {
                if (_rotation == QuarterRotation.Upright || _rotation == QuarterRotation.Half)
                {
                    _flipState.Y = !_flipState.Y;
                }
                else
                {
                    _flipState.X = !_flipState.X;
                }
            })
            .Add(Keys.Q, "Rotate CCW", false,() => _rotation = _rotation.CounterClockwisePrevious())
            .Add(Keys.E, "Rotate CW", false,() => _rotation = _rotation.ClockwiseNext())
        );

        element.AddButton(new Button(new GridPosition(1, 1), () => OpenShapeModal(screen))
            .SetTileStateGetter(GetForegroundShapeTileState)
            .SetTileStateOnHoverGetter(() => GetForegroundShapeTileState().WithForeground(Color.LimeGreen))
        );
        element.AddButton(
            new Button(new GridPosition(1, 2), () => OpenForegroundColorModal(screen))
                .SetTileStateGetter(GetForegroundColorTileState)
                .SetTileStateOnHoverGetter(() => GetForegroundColorTileState().WithSprite(ResourceAlias.Tools, 0))
        );
        element.AddButton(
            new Button(new GridPosition(1, 3), () => OpenBackgroundColorModal(screen))
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

    private static GridPosition PanelTopLeft(AsciiScreen screen)
    {
        return screen.RoomRectangle.TopRight + new GridPosition(-PanelSize.X, 0);
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

    private void OpenForegroundColorModal(AsciiScreen screen)
    {
        var modal = new ChooseColorModal(
            new GridRectangle(PanelTopLeft(screen) + new GridPosition(-ColorModalWidth(), 10), PanelTopLeft(screen))
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

    private void OpenBackgroundColorModal(AsciiScreen screen)
    {
        var modal = new ChooseColorModal(
            new GridRectangle(PanelTopLeft(screen) + new GridPosition(-ColorModalWidth(), 10), PanelTopLeft(screen))
                .Moved(new GridPosition(0, 2)),
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

    private void OpenShapeModal(AsciiScreen screen)
    {
        var chooseTileModal = new ChooseShapeModal(
            new GridRectangle(PanelTopLeft(screen), PanelTopLeft(screen) + new GridPosition(-15, 10)),
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

    /// <summary>
    ///     Gets the current Shape and Colors based on what filters are currently active
    /// </summary>
    public void SetBasedOnData(CanvasTileData tileData)
    {
        if (ForegroundShapeAndTransform.IsFunctionallyActive)
        {
            _currentShape = tileData.GetShape();
            _rotation = QuarterRotation.FromAngle(tileData.Angle);
            _flipState = new XyBool(tileData.FlipX, tileData.FlipY);
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
