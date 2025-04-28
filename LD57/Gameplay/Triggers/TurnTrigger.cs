namespace LD57.Gameplay.Triggers;

public record TurnTrigger : IBehaviorTrigger
{
    public static readonly TurnTrigger Instance = new();
}
