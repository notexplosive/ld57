namespace LD57.Gameplay;

public record struct MoveStatus()
{
    public void Fail()
    {
        WasSuccessful = false;
    }

    public bool WasSuccessful { get; private set; } = true;

    public void DependOnMove(MoveStatus cascadedMove)
    {
        if (!cascadedMove.WasSuccessful)
        {
            Fail();
        }
    }
}
