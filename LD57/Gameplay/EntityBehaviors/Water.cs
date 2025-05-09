﻿using LD57.Gameplay.Triggers;
using LD57.Rendering;

namespace LD57.Gameplay.EntityBehaviors;

public class Water : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is EntityMovedTrigger moveTrigger)
        {
            var mover = moveTrigger.Data.Mover;

            if (mover.Position != self.Position)
            {
                return;
            }

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
                mover.TweenableGlyph.SetAnimation(Animations.FloatOnWater());
            }
        }
    }
}
