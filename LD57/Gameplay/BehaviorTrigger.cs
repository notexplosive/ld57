namespace LD57.Gameplay;

public class BehaviorTrigger
{
    public static readonly BehaviorTriggerBasic OnEnter = new(nameof(OnEnter));
    public static readonly BehaviorTriggerBasic OnWorldStart = new(nameof(OnWorldStart));
    public static readonly BehaviorTriggerBasic OnSignalChange = new(nameof(OnSignalChange));
    public static readonly BehaviorTriggerBasic OnTurn = new(nameof(OnTurn));
    public static readonly BehaviorTriggerBasic OnReset = new(nameof(OnReset));
    public static readonly BehaviorTriggerWithEntity OnTouch = new(nameof(OnTouch));
    public static readonly BehaviorTriggerWithEntity OnEntityMoved = new(nameof(OnEntityMoved));
    public static readonly BehaviorTriggerWithKeyValuePair OnStateChanged = new(nameof(OnStateChanged));
    
}
