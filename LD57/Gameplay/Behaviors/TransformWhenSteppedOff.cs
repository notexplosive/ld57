using ExplogineMonoGame;
using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class TransformWhenSteppedOff : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is not SteppedOffTrigger steppedOffTrigger)
        {
            return;
        }

        var template = self.State.GetString("transform_to_template");

        if (template == null)
        {
            Client.Debug.LogWarning("Missing template to transform to");
            return;
        }

        if (!steppedOffTrigger.SteppingEntity.HasTag("Solid"))
        {
            return;
        }

        self.World.Destroy(self);
        self.World.AddEntity(self.World.CreateEntityFromTemplate(ResourceAlias.EntityTemplate(template), self.Position, []));
    }
}
