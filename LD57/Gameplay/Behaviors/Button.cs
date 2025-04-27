using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class Button : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        var channel = self.GetChannel();
        
        if (trigger is EntityMovedTrigger)
        {
            var entitiesInSameRoom = self.World.EntitiesInSameRoom(self.Position).ToList();
            var isPressed = entitiesInSameRoom
                .Where(a => a.Position == self.Position).Any(a => a.HasTag("PressesButtons"));

            var isInitialized = self.State.HasKey("is_pressed");
            var wasPressed = self.State.GetBool("is_pressed");
            self.State.Set("is_pressed", isPressed);

            if (isInitialized && wasPressed != isPressed)
            {
                if (isPressed)
                {
                    ResourceAlias.PlaySound("press_button", new SoundEffectSettings());
                }
                else
                {
                    ResourceAlias.PlaySound("press_button", new SoundEffectSettings{Pitch = 0.5f});
                }

                foreach (var otherEntities in entitiesInSameRoom.Where(other => other.GetChannel() == channel))
                {
                    otherEntities.TriggerBehavior(new SignalChangeTrigger());
                }
            }
        }
    }
}
