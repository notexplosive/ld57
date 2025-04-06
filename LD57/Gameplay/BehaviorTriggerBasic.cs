namespace LD57.Gameplay;

public class BehaviorTriggerBasic : IBehaviorTrigger
{
    private readonly string _debugName;

    public BehaviorTriggerBasic(string debugName)
    {
        _debugName = debugName;
    }

    public override string ToString()
    {
        return $"Basic:{_debugName}";
    }
}