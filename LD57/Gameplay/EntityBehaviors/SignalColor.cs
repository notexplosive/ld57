using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.EntityBehaviors;

public class SignalColor : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        var channel = self.GetChannel();

        if (trigger is not WorldStartTrigger)
        {
            return;
        }

        var signalColor = ResourceAlias.Color("signal_" + channel);
        self.Appearance.TileState =
            self.Appearance.TileState with {ForegroundColor = signalColor};
    }
}
