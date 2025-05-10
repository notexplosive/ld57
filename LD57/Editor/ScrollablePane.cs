using ExplogineMonoGame;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class ScrollablePane : UiElement, ISubElement
{
    /// <summary>
    ///     Viewport is the On-Screen Rectangle of the pane, it's in the coordinate space of the parent element (origin = top
    ///     left of the element)
    /// </summary>
    private readonly GridRectangle _viewport;

    private GridRectangle _contentBounds;

    public ScrollablePane(GridRectangle viewport) : base(viewport, false)
    {
        _viewport = viewport;
        // initially, the content is the same size as the viewport
        _contentBounds = viewport;
    }

    public void PutSubElementOnScreen(AsciiScreen screen, bool isHovered)
    {
        screen.PutFilledRectangle(TileState.BackgroundOnly(Color.Blue, 1f), _viewport);
        PaintUiElement(screen, new GridPosition()); // todo: make it possible to hover things in a scroll pane
    }

    public void OnClicked()
    {
    }

    public void SetContentHeight(int height)
    {
        _contentBounds = _contentBounds with {Height = height};
    }
}
