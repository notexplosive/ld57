using ExplogineCore.Data;
using ExplogineCore.Lua;
using JetBrains.Annotations;

namespace LD57.Tableau;

[LuaBoundType]
public class LuaNoise
{
    private readonly Noise _noise;

    public LuaNoise(Noise noise)
    {
        _noise = noise;
    }

    [UsedImplicitly]
    [LuaMember("integer")]
    public int Integer(int index, int max)
    {
        return _noise.PositiveIntAt(index, max);
    }

    [UsedImplicitly]
    [LuaMember("number")]
    public float Number(int index)
    {
        return _noise.FloatAt(index);
    }
}
