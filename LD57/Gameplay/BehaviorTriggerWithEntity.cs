namespace LD57.Gameplay;

public class BehaviorTriggerWithEntity : IBehaviorTrigger<BehaviorTriggerWithEntity.Payload>
{
    private readonly string _debugName;

    public BehaviorTriggerWithEntity(string debugName)
    {
        _debugName = debugName;
    }
    
    public override string ToString()
    {
        return $"Entity:{_debugName}";
    }
    
    public Payload CreateEmptyPayload()
    {
        return new Payload(null);
    }

    public record Payload(Entity? Entity) : IBehaviorTriggerPayload;
}