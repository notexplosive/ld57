using System.Text;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class DialogueBox
{
    private readonly TweenableRectangleF _rectangle = new(RectangleF.Empty);
    private readonly GridPosition _screenSize;
    private MessageContent? _currentMessage;
    private bool _isMessageVisible;
    private int _pageIndex;

    public DialogueBox(GridPosition screenSize)
    {
        _screenSize = screenSize;
    }

    public bool IsVisible { get; private set; }
    public bool IsClosed { get; private set; }

    private CallbackTween SetVisibilityCallback(bool isVisible)
    {
        return new CallbackTween(() => IsVisible = isVisible);
    }

    private CallbackTween SetTextVisibilityCallback(bool isVisible)
    {
        return new CallbackTween(() => _isMessageVisible = isVisible);
    }

    public void PaintToScreen(AsciiScreen screen)
    {
        if (IsVisible)
        {
            var topLeft = new GridPosition(_rectangle.Value.Location.Rounded().ToPoint());
            screen.PutFrameRectangle(ResourceAlias.PopupFrame,
                topLeft, new GridPosition(
                    (_rectangle.Value.Location + _rectangle.Value.Size).Rounded().ToPoint()));

            if (_isMessageVisible)
            {
                if (_currentMessage != null)
                {
                    var page = _currentMessage.GetPage(_pageIndex);
                    if (page != null)
                    {
                        page.PaintToScreen(topLeft + new GridPosition(1, 1), screen);
                    }
                }
            }
        }
    }

    public void ShowMessage(SequenceTween tween, MessageContent message)
    {
        IsClosed = false;
        _pageIndex = 0;
        _currentMessage = message;

        tween.Add(_rectangle.CallbackSetTo(SmallCenterRectangle()));

        var page = message.GetPage(0);

        if (page != null)
        {
            ShowPage(tween, page);
        }
        else
        {
            DoCloseAnimation(tween);
        }
    }

    public void NextPage(SequenceTween tween)
    {
        if (_currentMessage == null)
        {
            return;
        }

        if (!tween.IsDone())
        {
            return;
        }

        _pageIndex++;
        var page = _currentMessage.GetPage(_pageIndex);

        if (page != null)
        {
            ShowPage(tween, page);
        }
        else
        {
            DoCloseAnimation(tween);
        }
    }

    private Rectangle SmallCenterRectangle()
    {
        var screenRectangle = new Rectangle(Point.Zero, _screenSize.ToPoint());
        var startingRectangle = new Rectangle(screenRectangle.Center - new Point(0, 1),
            new Point(1, 1));
        return startingRectangle;
    }

    private void DoCloseAnimation(SequenceTween tween)
    {
        tween
            .Add(SetTextVisibilityCallback(false))
            .Add(_rectangle.TweenTo(SmallCenterRectangle(), 0.15f, Ease.Linear))
            .Add(SetVisibilityCallback(false))
            .Add(new CallbackTween(()=> IsClosed = true))
            ;
    }

    private void ShowPage(SequenceTween tween, MessagePage page)
    {
        var screenRectangle = new Rectangle(Point.Zero, _screenSize.ToPoint());

        var center = screenRectangle.Center;

        var textWidth = page.Width + 1;
        var height = page.Height + 1;

        var desiredRectangle = new Rectangle(center - new Point(textWidth / 2, 1), new Point(textWidth, height));
        tween
            .Add(SetVisibilityCallback(true))
            .Add(SetTextVisibilityCallback(false))
            .Add(_rectangle.TweenTo(desiredRectangle, 0.15f, Ease.QuadFastSlow))
            .Add(SetTextVisibilityCallback(true))
            ;
    }
}
