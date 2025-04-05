namespace LD57.Gameplay;

public record struct MoveStatus(bool WasInterrupted)
{
    public void Interrupt()
    {
        WasInterrupted = true;
    }

    public void DependOnMove(MoveStatus cascadedMove)
    {
        if (cascadedMove.WasInterrupted)
        {
            Interrupt();
        }
    }
}
