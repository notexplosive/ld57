using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace LD57.Sessions;

public class LdSession : Session
{
    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
    }

    public override void Update(float dt)
    {
    }

    public override void Draw(Painter painter)
    {
        var spriteFont = ResourceAlias.GameFont.GetFont(32);
        painter.BeginSpriteBatch();
        painter.SpriteBatch.DrawString(spriteFont, "Hello Game!", Vector2.Zero, Color.White);
        painter.EndSpriteBatch();
    }
}
