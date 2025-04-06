using LD57.Gameplay;
using Microsoft.Xna.Framework;

namespace TestLD57;

public class StateTests
{
    [Fact]
    public void GetColorFromState()
    {
        var state = new State();
        state.SetString("good", "ff0000");
        state.SetString("bad", "this is not a color");

        var good = state.GetColor("good");
        var bad = state.GetColor("bad");

        Assert.True(good.HasValue);
        Assert.False(bad.HasValue);

        Assert.Equivalent(good.Value, new Color(1f, 0f, 0f));
    }
}
