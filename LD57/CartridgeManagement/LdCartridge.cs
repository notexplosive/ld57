using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using LD57.Sessions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD57.CartridgeManagement;

public class LdCartridge(IRuntime runtime) : BasicGameCartridge(runtime)
{
    private EditorSession? _editorSession;
    private LdSession? _gameSession;
    private ISession? _session;

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080), SamplerState.LinearWrap);

    public override void OnCartridgeStarted()
    {
        _editorSession = new EditorSession((Runtime.Window as RealWindow)!, Runtime.FileSystem);
        _gameSession = new LdSession((Runtime.Window as RealWindow)!, Runtime.FileSystem);

        if (Client.Debug.IsPassiveOrActive)
        {
            _session = _editorSession;
        }
        else
        {
            // _gameSession.LoadCurrentLevel();
            _session = _gameSession;
        }
    }

    public override void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (input.Keyboard.GetButton(Keys.F4).WasPressed)
            {
                ToggleSession();
            }
        }

        _session?.UpdateInput(input, hitTestStack);
    }

    private void ToggleSession()
    {
        if (_session == _editorSession)
        {
            _session = _gameSession;
        }
        else
        {
            _session = _editorSession;
        }
    }

    public override void Update(float dt)
    {
        _session?.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        _session?.Draw(painter);
    }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override void OnHotReload()
    {
        _session?.OnHotReload();
    }

    public override IEnumerable<ILoadEvent> LoadEvents(Painter painter)
    {
        LdResourceAssets.Reset();
        foreach (var item in LdResourceAssets.Instance.LoadEvents(painter))
        {
            yield return item;
        }
    }
}
