using ExplogineMonoGame.Data;
using LD57.Rendering;

namespace LD57.Gameplay;

public readonly record struct MoveData(Entity Mover, GridPosition Source, GridPosition Destination, Direction Direction);
