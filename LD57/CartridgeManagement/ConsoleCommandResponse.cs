namespace LD57.CartridgeManagement;

public class ConsoleCommandResponse
{
    private readonly SessionSwitcherOverlay _switcher;

    public ConsoleCommandResponse(SessionSwitcherOverlay switcher)
    {
        _switcher = switcher;
    }

    public StatusType Status { private set; get; }

    public void SuccessAndClose(string message)
    {
        _switcher.Log(message);
        Status = StatusType.Success;
    }

    public enum StatusType
    {
        Uninitialized,
        Success,
        Failure,
        Unrecognized
    }

    public void FailUnrecognized()
    {
        Status = StatusType.Unrecognized;
    }
}
