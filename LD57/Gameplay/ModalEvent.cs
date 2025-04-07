namespace LD57.Gameplay;

public abstract record ModalEvent
{
    public bool IsRunning { get; private set; }

    public void Execute()
    {
        if (IsRunning == false)
        {
            ExecuteInternal();
        }

        IsRunning = true;
    }

    public bool IsDone()
    {
        return IsRunning && IsDoneInternal();
    }
    
    protected abstract void ExecuteInternal();

    public abstract bool IsDoneInternal();
}
