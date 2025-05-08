using ExplogineMonoGame.Data;
using ExTween;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class TitleCard
{
    private readonly TweenableInt _characterIndex = new();
    private readonly TweenableRectangleF _rectangle = new(RectangleF.Empty);
    private readonly GridPosition _screenSize;
    private string _currentMessage = string.Empty;
    private bool _isVisible;

    public TitleCard(GridPosition screenSize)
    {
        _screenSize = screenSize;
    }

    private CallbackTween SetVisibilityCallback(bool isVisible)
    {
        return new CallbackTween(() => _isVisible = isVisible);
    }

    public void PaintToScreen(AsciiScreen screen)
    {
        if (_isVisible)
        {
            screen.PutFrameRectangle(ResourceAlias.PopupFrame, GridRectangle.FromRectangleF(_rectangle));
            screen.PutString(new GridPosition(_rectangle.Value.Location.Rounded().ToPoint()) + new GridPosition(1, 1),
                _currentMessage.Substring(0, _characterIndex));
        }
    }

    public void DoAnimation(SequenceTween tween, string content)
    {
        var screenRectangle = new Rectangle(Point.Zero, _screenSize.ToPoint());

        var center = screenRectangle.Center;
        center -= new Point(0, 5);

        var textWidth = content.Length + 1;
        var height = 2;
        var startingRectangle = new Rectangle(center - new Point(0, 1),
            new Point(1, height));
        var desiredRectangle = new Rectangle(center - new Point(textWidth / 2, 1),
            new Point(textWidth, height));
        var expandDuration = 0.25f;
        var contractDuration = 0.25f;
        tween
            .Add(SetVisibilityCallback(true))
            .Add(_rectangle.CallbackSetTo(startingRectangle))
            .Add(_characterIndex.CallbackSetTo(0))
            .Add(new CallbackTween(() => { _currentMessage = content; }))
            .Add(new WaitSecondsTween(0.1f))
            .Add(
                new MultiplexTween()
                    .Add(_rectangle.TweenTo(desiredRectangle, expandDuration, Ease.QuadFastSlow))
            )
            .Add(_characterIndex.CallbackSetTo(content.Length))
            .Add(new WaitSecondsTween(1.5f))
            .Add(_characterIndex.CallbackSetTo(0))
            .Add(new MultiplexTween()
                .Add(_rectangle.TweenTo(startingRectangle, contractDuration, Ease.QuadSlowFast))
            )
            .Add(new WaitSecondsTween(0.1f))
            .Add(SetVisibilityCallback(false))
            ;
    }
}
