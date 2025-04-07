using ExTween;

namespace LD57.Gameplay;

public interface ITransition
{
    ITween FadeIn();
    ITween FadeOut();
    void PaintToScreen(float dt);
}
