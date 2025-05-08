using System;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class CanvasBrushMode
{
    public CanvasBrushLayer ForegroundShapeAndTransform { get; set; } = new(true, true);
    public CanvasBrushLayer ForegroundColor { get; set; } = new(true, true);
    public CanvasBrushLayer BackgroundColorAndIntensity { get; set; } = new(true, true);

    public CanvasTileData GetTile()
    {
        // todo!
        return new CanvasTileData();
    }

    public UiElement CreateUi(AsciiScreen screen)
    {
        var panelSize = new GridPosition(4, 5);
        var topLeft = screen.Rectangle.TopRight + new GridPosition(-panelSize.X, 0);
        var element = new UiElement(new GridRectangle(topLeft, topLeft + panelSize + new GridPosition(1,1)));

        element.AddDynamicTile(new GridPosition(1, 1), GetCurrentTileState);

        element.AddButton(new Button(new GridPosition(1,2), OpenForegroundTileStateModal).SetTileStateGetter(GetForegroundTileStateWithoutColor));
        element.AddDynamicTile(new GridPosition(1, 3), GetForegroundColorTileState);
        element.AddDynamicTile(new GridPosition(1, 4), GetBackgroundTileState);

        element.AddDynamicTile(new GridPosition(2, 2), GetVisibleTileState(()=>ForegroundShapeAndTransform.IsVisible));
        element.AddDynamicTile(new GridPosition(2, 3), GetVisibleTileState(()=>ForegroundColor.IsVisible));
        element.AddDynamicTile(new GridPosition(2, 4), GetVisibleTileState(()=>BackgroundColorAndIntensity.IsVisible));
        
        element.AddDynamicTile(new GridPosition(3, 2), GetEditingTileState(()=>ForegroundShapeAndTransform.IsEditing));
        element.AddDynamicTile(new GridPosition(3, 3), GetEditingTileState(()=>ForegroundColor.IsEditing));
        element.AddDynamicTile(new GridPosition(3, 4), GetEditingTileState(()=>BackgroundColorAndIntensity.IsEditing));

        return element;
    }

    private void OpenForegroundTileStateModal()
    {
        RequestModal?.Invoke(new ChooseTileModal(new GridRectangle(new GridPosition(5,5), new GridPosition(20, 20))));
    }

    public event Action<Popup>? RequestModal;

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
        return TileState.BackgroundOnly(Color.Orange, 0.5f);
    }

    private TileState GetForegroundColorTileState()
    {
        return TileState.BackgroundOnly(Color.LightBlue, 1f);
    }

    private TileState? GetForegroundTileStateWithoutColor()
    {
        return TileState.Sprite(ResourceAlias.Entities, 0, Color.White);
    }
}