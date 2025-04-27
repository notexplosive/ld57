using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public interface IEntityBehavior
{
    void OnTrigger(Entity self, IBehaviorTrigger trigger);
}
