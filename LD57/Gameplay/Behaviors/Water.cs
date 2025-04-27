using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class Water : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is EntityMovedTrigger movedTrigger)
        {
            var mover = movedTrigger.Mover;

            if (mover.Position == self.Position)
            {
                if (mover.HasTag("FillsWater"))
                {
                    self.World.Destroy(self);
                }

                if (mover.HasTag("DestroyInWater"))
                {
                    self.World.Destroy(mover);
                }

                if (mover.HasTag("DeactivateInWater"))
                {
                    mover.SetActive(false);
                }
            }
        }
    }
}
