using LD57.Gameplay;

namespace LD57.Rules;

public abstract class GameRule : IGameRule
{
    public virtual void OnMoveCompleted(World world, MoveData moveData)
    {
        
    }

    public virtual bool ShouldInterruptMove(World world, MoveData moveData)
    {
        return false;
    }
}
