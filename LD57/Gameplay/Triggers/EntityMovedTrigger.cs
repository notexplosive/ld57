using LD57.Rendering;

namespace LD57.Gameplay.Triggers;

public record EntityMovedTrigger(Entity Mover, MoveData Data) : IBehaviorTrigger;
