using ExplogineMonoGame;
using ExplogineMonoGame.Debugging;
using LD57.Rendering;

namespace LD57.Editor;

public class ScrollablePane : UiElement, ISubElement
{
    /// <summary>
    ///     Viewport is the On-Screen Rectangle of the pane, it's in the coordinate space of the parent element (origin = top
    ///     left of the element)
    /// </summary>
    private readonly GridRectangle _viewport;

    private GridRectangle _contentBounds;

    private GridPosition _viewPosition;

    public ScrollablePane(GridRectangle viewport) : base(viewport, false)
    {
        _viewport = viewport;

        // initially, the content is the same size as the viewport
        _contentBounds = viewport;
    }

    public GridRectangle ViewRectangle => _viewport.MovedToZero().Moved(_viewPosition);
    public GridRectangle ViewPort => _viewport;

    public void PutSubElementOnScreen(AsciiScreen screen, ISubElement? hoveredElement)
    {
        screen.PushStencil(_viewport);
        PaintSubElements(screen, hoveredElement);
        screen.PopStencil();
    }

    public void OnClicked()
    {
        // do nothing, clicking the scroll area directly doesn't do anything
    }

    public void OnScroll(int scrollDelta, ISubElement? hoveredElement)
    {
        _viewPosition += new GridPosition(0, scrollDelta);
        ClampViewPosition();
    }

    private void ClampViewPosition()
    {
        if (_viewPosition.Y < 0)
        {
            _viewPosition = _viewPosition with {Y = 0};
        }

        var bottom = ContentBottom();
        if (_viewPosition.Y > bottom)
        {
            _viewPosition = _viewPosition with {Y = bottom};
        }
    }

    private int ContentBottom()
    {
        return _contentBounds.Bottom - _viewport.Height - 2;
    }

    public void SetContentHeight(int height)
    {
        _contentBounds = _contentBounds with {Height = height};
    }

    protected override GridPosition AdditionalTransform()
    {
        return -_viewPosition;
    }

    public void ScrollToPosition(int y)
    {
        _viewPosition = _viewPosition with {Y = y - _viewport.Height};
        ClampViewPosition();
    }

    public int ThumbPosition(int barHeight)
    {
        if (!ShouldHaveThumb())
        {
            return 0;
        }

        var scrollPositionPercent = (float)_viewPosition.Y / ContentBottom();
        return (int)(scrollPositionPercent * barHeight);
    }

    public bool ShouldHaveThumb()
    {
        return _viewport.Height < _contentBounds.Bottom;
    }
}
