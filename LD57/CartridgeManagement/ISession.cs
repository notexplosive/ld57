using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD57.Rendering;

namespace LD57.CartridgeManagement;

public interface ISession
{
    void OnHotReload();
    void UpdateInput(ConsumableInput input, HitTestStack hitTestStack);
    void Update(float dt);
    void Draw(Painter painter);
    
    AsciiScreen Screen { get; }
}
