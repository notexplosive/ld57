using ExplogineMonoGame;
using ExplogineMonoGame.Data;

namespace LD57.CartridgeManagement;

public interface ISession
{
    void OnHotReload();
    void UpdateInput(ConsumableInput input, HitTestStack hitTestStack);
    void Update(float dt);
    void Draw(Painter painter);
}
