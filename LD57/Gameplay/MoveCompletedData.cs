using LD57.Rendering;

namespace LD57.Gameplay;

public readonly record struct MoveCompletedData(Entity Mover, GridPosition OldPosition, GridPosition NewPosition);
