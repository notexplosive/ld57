using ExTween;

namespace LD57.Gameplay;

public record DialogueModalEvent(MessageContent Message, DialogueBox DialogueBox, SequenceTween Tween) : ModalEvent
{
    protected override void ExecuteInternal()
    {
        Tween.SkipToEnd();
        DialogueBox.ShowMessage(Tween, Message);
    }

    public override bool IsDoneInternal()
    {
        return DialogueBox.IsClosed;
    }
}
