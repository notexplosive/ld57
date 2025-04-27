using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class Water : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is TouchTrigger touchTrigger)
        {
            var touchingEntity = touchTrigger.TouchingEntity;

            if (touchingEntity.HasTag("FillsWater") && touchingEntity.HasTag("FloatsInWater"))
            {
                self.World.Destroy(self);
                touchingEntity.SetActive(false);
            }

            if (touchingEntity.HasTag("FillsWater") && !touchingEntity.HasTag("FloatsInWater"))
            {
                self.World.Destroy(self);
                self.World.Destroy(touchingEntity);
            }
        }
    }
}
