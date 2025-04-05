using LD57.Gameplay;

namespace LD57.Rules;

public interface IGameRule
{
    void OnMoveCompleted(World world, MoveData moveData);
}