using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Sessions;

public class LdSession : Session
{
    private readonly AsciiScreen _screen;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem) : base(runtimeWindow,
        runtimeFileSystem)
    {
        _screen = new AsciiScreen(40, 22, 48);
    }

    public override void OnHotReload()
    {
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
    }

    public override void Update(float dt)
    {
        _screen.Clear(TileState.Empty);
        
        var bottomHudTopLeft = new GridPosition(0, 19);
        _screen.PutRectangle(ResourceAlias.PopupFrame, bottomHudTopLeft,
            bottomHudTopLeft + new GridPosition(_screen.Width - 1, 2));
        _screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), "Status: OK");
        _screen.PutString(bottomHudTopLeft + new GridPosition(2, 0), "Z[ ]");
        _screen.SetTile(bottomHudTopLeft + new GridPosition(4, 0), TileState.Sprite(ResourceAlias.Entities, 14));
        _screen.PutString(bottomHudTopLeft + new GridPosition(7, 0), "X[ ]", Color.Gray);
        _screen.SetTile(bottomHudTopLeft + new GridPosition(9, 0), TileState.Sprite(ResourceAlias.Entities, 15));
    }

    public override void Draw(Painter painter)
    {
        _screen.Draw(painter);
    }
}
