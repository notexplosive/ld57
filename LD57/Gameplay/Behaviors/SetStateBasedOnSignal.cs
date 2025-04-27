using System.Linq;
using ExplogineMonoGame;
using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class SetStateBasedOnSignal : IEntityBehavior
{
    private readonly string _keyName;
    private readonly string _invertedKey;

    public SetStateBasedOnSignal(string keyName, string invertedKey)
    {
        _keyName = keyName;
        _invertedKey = invertedKey;
    }
    
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        var channel = self.GetChannel();

        if (trigger is not SignalChangeTrigger)
        {
            return;
        }

        var shouldOpen = self.AllButtonsPressedForChannel(channel);

        if (self.State.GetBoolOrFallback(_invertedKey, false))
        {
            shouldOpen = !shouldOpen;
        }

        self.State.Set(_keyName, shouldOpen);
    }
}