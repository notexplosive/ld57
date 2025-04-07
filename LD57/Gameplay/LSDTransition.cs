using System;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExTween;
using LD57.CartridgeManagement;
using LD57.Rendering;

namespace LD57.Gameplay;

public class LsdTransition : ITransition
{
    private readonly AsciiScreen _screen;
    private readonly TweenableFloat _percentOfTilesToShow = new(1f);

    public LsdTransition(AsciiScreen screen)
    {
        _screen = screen;
    }
    
    public void PaintToScreen(float dt)
    {
        var targetTilePositions = _screen.AllTiles().ToList();
        new NoiseBasedRng(Client.Random.CleanNoise).Shuffle(targetTilePositions);
        for (var index = 0; index < targetTilePositions.Count * _percentOfTilesToShow; index++)
        {
            var targetTilePosition = targetTilePositions[index];
            var noise = new Noise((int) (Client.TotalElapsedTime * 2 +
                                         (Math.Sin(targetTilePosition.X) + Math.Sin(targetTilePosition.Y))));
            var random = new NoiseBasedRng(noise);
            
            var isLetter = random.NextFloat() < 0.25f;
            TileState tile;
            var color = random.GetRandomElement(LdResourceAssets.Instance.AllKnownColors.ToList());

            if (isLetter)
            {
                tile = TileState.StringCharacter(random.NextPrintableAsciiChar().ToString().ToUpper(), color);
            }
            else
            {
                var sheet = random.GetRandomElement(ResourceAlias.Sheets().ToList());
                tile = TileState.Sprite(sheet, random.NextPositiveInt(), color);
            }

            _screen.PutTile(targetTilePosition, tile);
        }
    }

    public ITween FadeIn()
    {
        return new SequenceTween()
            .Add(_percentOfTilesToShow.CallbackSetTo(0))
            .Add(_percentOfTilesToShow.TweenTo(1f, 0.5f, Ease.QuadFastSlow));
    }
    
    public ITween FadeOut()
    {
        return new SequenceTween()
            .Add(_percentOfTilesToShow.CallbackSetTo(1))
            .Add(_percentOfTilesToShow.TweenTo(0, 1f, Ease.QuadFastSlow));
    }
}
