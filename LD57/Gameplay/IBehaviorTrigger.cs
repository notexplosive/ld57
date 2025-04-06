namespace LD57.Gameplay;

public interface IBehaviorTrigger<out TPayload> : IBehaviorTrigger where TPayload : IBehaviorTriggerPayload
{
    TPayload CreateEmptyPayload();
}

public interface IBehaviorTrigger;
