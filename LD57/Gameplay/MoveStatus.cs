namespace LD57.Gameplay;

public record struct MoveStatus(MoveData MoveData)
{
    public void Interrupt()
    {
        WasInterrupted = true;
    }

    public bool WasInterrupted { get; set; }

    public void DependOnMove(MoveStatus cascadedMove)
    {
        if (cascadedMove.WasInterrupted)
        {
            Interrupt();
        }
    }
}
