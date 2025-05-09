﻿using LD57.Rendering;

namespace LD57.Gameplay;

public interface IEntityAppearance
{
    TileState TileState { get; set; }
    public int RawSortPriority { get; }
}
