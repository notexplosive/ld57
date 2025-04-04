﻿using ExplogineDesktop;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using LD57;
using LD57.CartridgeManagement;
using Microsoft.Xna.Framework;

var config = new WindowConfigWritable
{
    WindowSize = new Point(1600, 900),
    Title = "NotExplosive.net"
};
Bootstrap.Run(args, new WindowConfig(config), runtime => new HotReloadCartridge(runtime, new LdCartridge(runtime)));