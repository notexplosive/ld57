using LD57.Rendering;

namespace TestLD57;

public class GridRectangleTests
{
    [Fact]
    public void CreateRectangleFromCorners()
    {
        var gridRectangle = new GridRectangle(new GridPosition(1, 0), new GridPosition(3, 4));
        
        Assert.Equal(1, gridRectangle.Left);
        Assert.Equal(0, gridRectangle.Top);
        Assert.Equal(3, gridRectangle.Right);
        Assert.Equal(4, gridRectangle.Bottom);
        
        Assert.Equal(2, gridRectangle.Width);
        Assert.Equal(4, gridRectangle.Height);
        Assert.Equal(15, gridRectangle.AllPositions().Count());
    }

    [Fact]
    public void SingleCellRectangle()
    {
        var gridRectangle = new GridRectangle(new GridPosition(1, 1), new GridPosition(1,1));
        
        Assert.Equal(1, gridRectangle.Width);
        Assert.Equal(1, gridRectangle.Height);
        Assert.Single(gridRectangle.AllPositions());
    }
}
