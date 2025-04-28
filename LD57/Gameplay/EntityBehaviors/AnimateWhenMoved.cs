using LD57.Gameplay.Triggers;
using LD57.Rendering;

namespace LD57.Gameplay.EntityBehaviors;

public class AnimateWhenMoved : IEntityBehavior
{
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is EntityMovedTrigger entityMovedTrigger && entityMovedTrigger.Data.Mover == self)
        {
            self.TweenableGlyph.AddAnimation(Animations.MakeMoveNudge(entityMovedTrigger.Data.Direction, Constants.TileSize / 4f));
        }
    }
}
