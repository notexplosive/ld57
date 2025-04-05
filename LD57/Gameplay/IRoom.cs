using System.Collections.Generic;

namespace LD57.Gameplay;

public interface IRoom
{
    IEnumerable<Entity> AllEntities();
}
