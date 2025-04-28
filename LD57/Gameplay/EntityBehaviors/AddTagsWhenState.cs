using System.Collections.Generic;
using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.EntityBehaviors;

public class AddTagsWhenState : IEntityBehavior
{
    private readonly string _stateKey;
    private readonly string[] _tags;
    private readonly bool _targetValue;

    public AddTagsWhenState(string stateKey, bool targetValue, string[] tags)
    {
        _stateKey = stateKey;
        _tags = tags;
        _targetValue = targetValue;
    }

    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is not StateChangeTrigger stateChangeTrigger)
        {
            return;
        }

        if (stateChangeTrigger.Key != _stateKey)
        {
            return;
        }

        var isCorrectTruthiness = self.State.GetBoolOrFallback(_stateKey, false) == _targetValue;

        foreach (var tag in _tags)
        {
            if (isCorrectTruthiness)
            {
                self.AddTag(tag);
            }
            else
            {
                self.RemoveTag(tag);
            }
        }
    }
}
