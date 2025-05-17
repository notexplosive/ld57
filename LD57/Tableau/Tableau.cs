using System;
using ExplogineCore.Lua;
using LD57.Rendering;
using MoonSharp.Interpreter;

namespace LD57.Tableau;

public class Tableau
{
    private readonly AsciiScreen _screen;
    private readonly LuaIpsum _ipsum;
    private readonly Func<float, DynValue>? _updateFunction;

    public Tableau(AsciiScreen screen, LuaRuntime luaRuntime, LuaIpsum ipsum)
    {
        _screen = screen;
        _ipsum = ipsum;
        
        if (ipsum.Setup != null)
        {
            luaRuntime.SafeCallFunction(ipsum.Setup);
        }
        
        if (ipsum.Update != null)
        {
            _updateFunction = dt => luaRuntime.SafeCallFunction(ipsum.Update, dt);
        }

        _screen.SetWidth(ipsum.Width);
    }

    public void Update(float dt)
    {
        if (_ipsum.Width != _screen.Width)
        {
            _screen.SetWidth(_ipsum.Width);
        }
        
        _updateFunction?.Invoke(dt);
    }
}
