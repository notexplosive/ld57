using System.Linq;
using LD57.Rendering;

namespace LD57.Gameplay.EntityBehaviors;

public static class BehaviorUtilities
{
    public static int GetChannel(this Entity self)
    {
        return self.State.GetIntOrFallback("channel", 0);
    }

    public static bool AllButtonsPressedForChannel(this Entity entity, int channel)
    {
        var relevantButtons = entity.World.EntitiesInSameRoom(entity.Position)
            .Where(other => other.HasTag("Button") && other.GetChannel() == channel).ToList();
        
        if (relevantButtons.Count == 0)
        {
            return false;
        }

        return relevantButtons.All(a => a.State.GetBool("is_pressed") == true);
    }
}
