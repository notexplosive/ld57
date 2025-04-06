using System;

namespace LD57.Gameplay;

public class EntityBehavior
{
    private readonly Action<IBehaviorTriggerPayload> _action;

    public EntityBehavior(IBehaviorTrigger trigger, Action<IBehaviorTriggerPayload> action)
    {
        Trigger = trigger;
        _action = action;
    }

    public IBehaviorTrigger Trigger { get; }

    public void DoAction(IBehaviorTriggerPayload payload)
    {
        _action(payload);
    }

    public void DoActionEmptyPayload()
    {
        _action(new EmptyBehaviorPayload());
    }
}
