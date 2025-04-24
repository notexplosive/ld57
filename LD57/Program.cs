using ExplogineDesktop;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using LD57;
using LD57.CartridgeManagement;
using Microsoft.Xna.Framework;

var config = new WindowConfigWritable
{
    WindowSize = new Point(1600, 900),
    Title = Constants.Title
};

Client.SetLoadingCartridgeFactory((runtime, loader) => new AsciiLoadingCartridge(runtime, loader));
Client.SetIntroCartridgeFactory(runtime => new AsciiIntroCartridge(runtime));

Bootstrap.Run(args, new WindowConfig(config), runtime => new HotReloadCartridge(runtime, new LdCartridge(runtime)));
