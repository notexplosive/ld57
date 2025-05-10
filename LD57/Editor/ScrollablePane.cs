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

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        screen.PutFilledRectangle(TileState.BackgroundOnly(Color.Blue, 1f), _viewport);
        PaintUiElement(screen, hoveredElement);
    }

    public void OnClicked()
    {
        // do nothing, clicking the scroll area directly doesn't do anything
    }

    public void SetContentHeight(int height)
    {
        _contentBounds = _contentBounds with {Height = height};
    }
}
