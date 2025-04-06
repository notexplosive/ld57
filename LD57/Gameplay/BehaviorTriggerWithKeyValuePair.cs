namespace LD57.Gameplay;

public class BehaviorTriggerWithKeyValuePair : IBehaviorTrigger<BehaviorTriggerWithKeyValuePair.Payload>
{
    private readonly string _debugName;

    public BehaviorTriggerWithKeyValuePair(string debugName)
    {
        _debugName = debugName;
    }
    
    public override string ToString()
    {
        return $"String:{_debugName}";
    }
    
    public Payload CreateEmptyPayload()
    {
        return new Payload("?", "?");
    }

    public record Payload(string Key, string Value) : IBehaviorTriggerPayload;
}