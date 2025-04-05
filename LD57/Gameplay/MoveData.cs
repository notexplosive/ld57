﻿using ExplogineMonoGame.Data;
using LD57.Rendering;

namespace LD57.Gameplay;

public readonly record struct MoveData(Entity Mover, GridPosition OldPosition, GridPosition NewPosition, Direction Direction);
