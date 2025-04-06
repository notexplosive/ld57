using System;

namespace LD57.Gameplay;

public class EntityBehavior
{
    private readonly Action _action;

    public EntityBehavior(BehaviorTrigger trigger, Action action)
    {
        Trigger = trigger;
        _action = action;
    }

    public BehaviorTrigger Trigger { get; }

    public void DoAction()
    {
        _action();
    }
}
