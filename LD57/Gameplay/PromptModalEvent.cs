using ExTween;

namespace LD57.Gameplay;

public record PromptModalEvent(PromptBox PromptBox,Prompt Prompt, SequenceTween Tween) : ModalEvent
{
    protected override void ExecuteInternal()
    {
        PromptBox.ShowPrompt(Tween, Prompt);
    }

    public override bool IsDoneInternal()
    {
        return PromptBox.HasMadeAChoice && !PromptBox.IsVisible;
    }
}
