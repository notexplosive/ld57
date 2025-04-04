using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD57.CartridgeManagement;

namespace LD57.Sessions;

public abstract class Session : ISession
{
    protected Session(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        RuntimeWindow = runtimeWindow;
        RuntimeFileSystem = runtimeFileSystem;
    }

    protected RealWindow RuntimeWindow { get; }
    protected ClientFileSystem RuntimeFileSystem { get; }

    public virtual void OnHotReload()
    {
    }

    public virtual void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
    }

    public virtual void Update(float dt)
    {
    }

    public virtual void Draw(Painter painter)
    {
    }
}
