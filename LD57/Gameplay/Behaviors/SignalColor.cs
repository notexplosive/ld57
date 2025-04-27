using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class SignalColor : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        var channel = self.GetChannel();
        
        if (trigger is WorldStartTrigger)
        {
            var signalColor = ResourceAlias.Color("signal_" + channel);
            if (self.Appearance != null && self.Appearance.TileState.HasValue)
            {
                self.Appearance.TileState =
                    self.Appearance.TileState.Value with {ForegroundColor = signalColor};
            }
        }
    }
}
