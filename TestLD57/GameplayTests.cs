using ExplogineMonoGame.Data;
using LD57.Gameplay;
using LD57.Rendering;
using LD57.Rules;

namespace TestLD57;

public class GameplayTests
{
    private readonly World _world;

    public GameplayTests()
    {
        _world = new World(new GridPosition(10, 10));
    }
    
    [Fact]
    public void BasicPush()
    {
        _world.Rules.AddRule(new PushPushables());
        var pusher = _world.AddEntity(new Entity(new GridPosition(0, 0), new Invisible()));
        pusher.AddTag("Pusher");
        
        var pushable = _world.AddEntity(new Entity(new GridPosition(1, 0), new Invisible()));
        pushable.AddTag("Pushable");

        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);
        
        Assert.Equivalent(new GridPosition(1,0), pusher.Position);
        Assert.Equivalent(new GridPosition(2,0), pushable.Position);
    }
    
    [Fact]
    public void ChainPush()
    {
        _world.Rules.AddRule(new PushPushables());
        var pusher = _world.AddEntity(new Entity(new GridPosition(0, 0), new Invisible()));
        pusher.AddTag("Pusher");
        
        var pushable = _world.AddEntity(new Entity(new GridPosition(1, 0), new Invisible()));
        pushable.AddTag("Pushable");
        
        var pushable2 = _world.AddEntity(new Entity(new GridPosition(2, 0), new Invisible()));
        pushable2.AddTag("Pushable");

        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);
        
        // nothing moved
        Assert.Equivalent(new GridPosition(0,0), pusher.Position);
        Assert.Equivalent(new GridPosition(1,0), pushable.Position);
        Assert.Equivalent(new GridPosition(2,0), pushable2.Position);
    }
}
