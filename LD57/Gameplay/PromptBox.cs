using System;
using ExplogineCore.Data;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Gameplay;

public class PromptBox
{
    private readonly TweenableRectangleF _rectangle = new(RectangleF.Empty);
    private readonly GridPosition _screenSize;
    private readonly TweenableGlyph _selectionCursorGlyphLeft = new();
    private readonly TweenableGlyph _selectionCursorGlyphRight = new();
    private Prompt? _currentPrompt;
    private bool _isTextVisible;
    private int _selectedIndex;
    private float _timer;

    public PromptBox(GridPosition screenSize)
    {
        _screenSize = screenSize;
    }

    public bool HasMadeAChoice { get; private set; }

    public bool IsVisible { get; private set; }

    private CallbackTween SetVisibilityCallback(bool isVisible)
    {
        return new CallbackTween(() => IsVisible = isVisible);
    }

    private CallbackTween SetTextVisibilityCallback(bool isVisible)
    {
        return new CallbackTween(() =>
        {
            _isTextVisible = isVisible;
            _timer = 0;
        });
    }

    public void PaintToScreen(AsciiScreen screen)
    {
        if (IsVisible && _currentPrompt != null)
        {
            var topLeft = new GridPosition(_rectangle.Value.Location.Rounded().ToPoint());
            screen.PutFrameRectangle(ResourceAlias.PopupFrame,
                topLeft, new GridPosition(
                    (_rectangle.Value.Location + _rectangle.Value.Size).Rounded().ToPoint()));

            if (_isTextVisible)
            {
                screen.PutSequence(topLeft + new GridPosition(1, 1), _currentPrompt.Title);

                if (_currentPrompt.Orientation == Orientation.Vertical)
                {
                    for (var index = 0; index < _currentPrompt.Options.Count; index++)
                    {
                        DrawOption(screen, topLeft + new GridPosition(1 + 1, 3 + index), _currentPrompt.Options[index],
                            _selectedIndex == index);
                    }
                }
                else
                {
                    var x = 1;
                    for (var index = 0; index < _currentPrompt.Options.Count; index++)
                    {
                        var option = _currentPrompt.Options[index];
                        DrawOption(screen, topLeft + new GridPosition(x + 1, 3), _currentPrompt.Options[index],
                            _selectedIndex == index);
                        x += option.Width() + 1;
                    }
                }
            }
        }
    }

    private void DrawOption(AsciiScreen screen, GridPosition position, PromptOption option, bool isSelected)
    {
        if (_currentPrompt != null)
        {
            screen.PutSequence(position, option.Text);

            if (isSelected)
            {
                var color = ResourceAlias.Color("player");
                screen.PutTile(position - new GridPosition(1, 0),
                    TileState.StringCharacter(CursorCharacter()) with {ForegroundColor = color},
                    _selectionCursorGlyphLeft);

                var x = 0;
                foreach (var tile in option.Text)
                {
                    screen.PutTile(position + new GridPosition(x, 0), tile with {ForegroundColor = color});
                    x++;
                }
            }
        }
    }

    private string CursorCharacter()
    {
        char[] characters = ['>', '>', ')', ']'];
        var sin = Math.Clamp(MathF.Sin(_timer * MathF.PI * 2 / characters.Length * 7), 0, 1);
        var index = (int) (sin * characters.Length);

        if (characters.IsValidIndex(index))
        {
            return characters[index].ToString();
        }

        return "-";
    }

    private Rectangle SmallCenterRectangle()
    {
        var screenRectangle = new Rectangle(Point.Zero, _screenSize.ToPoint());
        var startingRectangle = new Rectangle(screenRectangle.Center - new Point(0, 1),
            new Point(1, 1));
        return startingRectangle;
    }

    public void DoCloseAnimation(SequenceTween tween)
    {
        tween
            .Add(SetTextVisibilityCallback(false))
            .Add(_rectangle.TweenTo(SmallCenterRectangle(), 0.15f, Ease.Linear))
            .Add(SetVisibilityCallback(false))
            ;
    }

    public void ShowPrompt(SequenceTween tween, Prompt prompt)
    {
        tween.SkipToEnd();
        HasMadeAChoice = false;
        _selectedIndex = 0;
        _currentPrompt = prompt;

        var desiredRectangle = DesiredRectangle();

        tween
            .Add(SetTextVisibilityCallback(false))
            .Add(SetVisibilityCallback(true))
            .Add(_rectangle.CallbackSetTo(SmallCenterRectangle()))
            .Add(_rectangle.TweenTo(desiredRectangle, 0.25f, Ease.QuadFastSlow))
            .Add(new WaitSecondsTween(0.15f))
            .Add(SetTextVisibilityCallback(true))
            ;
    }

    private RectangleF DesiredRectangle()
    {
        if (_currentPrompt == null)
        {
            throw new Exception("Requested desired rectangle when there is no prompt");
        }

        return RectangleF.FromCenterAndSize(_screenSize.ToPoint().ToVector2() / 2f,
            new Vector2(_currentPrompt.Width(), _currentPrompt.Height()));
    }

    public void DoInput(Direction inputDirection, ActionButton actionButton)
    {
        if (_currentPrompt != null)
        {
            if (inputDirection != Direction.None)
            {
                if (_currentPrompt.Orientation == Orientation.Vertical)
                {
                    MoveSelection(inputDirection.ToPoint().Y);
                    AnimateCursorMove(inputDirection);
                }

                if (_currentPrompt.Orientation == Orientation.Horizontal)
                {
                    MoveSelection(inputDirection.ToPoint().X);
                    AnimateCursorMove(inputDirection);
                }
            }

            if (actionButton == ActionButton.Primary)
            {
                _currentPrompt.Options[_selectedIndex].Choose();
                HasMadeAChoice = true;
            }
        }
    }

    private void AnimateCursorMove(Direction inputDirection)
    {
        _selectionCursorGlyphLeft.RootTween.Add(new SequenceTween()
                .Add(_selectionCursorGlyphLeft.PixelOffset.TweenTo(inputDirection.ToGridCellSizedVector(10), 0.05f,
                    Ease.QuadFastSlow))
                .Add(_selectionCursorGlyphLeft.PixelOffset.TweenTo(new Vector2(0, 0), 0.15f, Ease.QuadFastSlow))
            )
            ;
    }

    private void MoveSelection(int delta)
    {
        _selectedIndex += delta;
        if (_selectedIndex < 0)
        {
            _selectedIndex = 0;
        }

        if (_currentPrompt != null)
        {
            var max = _currentPrompt.Options.Count - 1;
            if (_selectedIndex > max)
            {
                _selectedIndex = max;
            }
        }

        _timer = 0;
    }

    public void Update(float dt)
    {
        _timer += dt;
        _selectionCursorGlyphLeft.RootTween.Update(dt);
        _selectionCursorGlyphRight.RootTween.Update(dt);
    }
}
