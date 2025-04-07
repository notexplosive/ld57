using System.Linq;
using ExplogineMonoGame.Data;
using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public class WipeTransition : ITransition
{
    private readonly AsciiScreen _screen;
    private readonly TileState _tileState;
    private readonly TweenableFloat _rightEdgePercent = new();
    private readonly TweenableFloat _leftEdgePercent = new();

    public WipeTransition(AsciiScreen screen, TileState tileState)
    {
        _screen = screen;
        _tileState = tileState;
    }

    public ITween FadeIn()
    {
        return new SequenceTween()
            .Add(_rightEdgePercent.CallbackSetTo(0))
            .Add(_leftEdgePercent.CallbackSetTo(0))
            .Add(_rightEdgePercent.TweenTo(1f, 0.5f, Ease.Linear));
    }

    public ITween FadeOut()
    {
        return new SequenceTween()
            .Add(_rightEdgePercent.CallbackSetTo(1))
            .Add(_leftEdgePercent.CallbackSetTo(0))
            .Add(_leftEdgePercent.TweenTo(1, 0.5f, Ease.Linear));
    }

    public void PaintToScreen(float dt)
    {
        var targetTilePositions = _screen.AllTiles().ToList();
        for (int index =(int)(targetTilePositions.Count * _leftEdgePercent); index < targetTilePositions.Count * _rightEdgePercent; index++)
        {
            var targetTilePosition = targetTilePositions[index];
            _screen.PutTile(targetTilePosition, _tileState);
        }
    }
}
