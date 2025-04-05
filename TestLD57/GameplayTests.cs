using ExplogineMonoGame.Data;
using LD57.Gameplay;
using LD57.Rendering;

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
        var pusher = _world.AddEntity(new Entity(new GridPosition(0, 0), new Invisible()))
                .AddTag("Pusher")
                .AddTag("Solid")
            ;

        var pushable = _world.AddEntity(new Entity(new GridPosition(1, 0), new Invisible()))
                .AddTag("Pushable")
                .AddTag("Solid")
            ;

        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);

        Assert.Equivalent(new GridPosition(1, 0), pusher.Position);
        Assert.Equivalent(new GridPosition(2, 0), pushable.Position);
    }

    [Fact]
    public void ChainPush_NothingMoves()
    {
        var pusher = _world.AddEntity(new Entity(new GridPosition(0, 0), new Invisible()))
                .AddTag("Pusher")
                .AddTag("Solid")
            ;

        var pushable = _world.AddEntity(new Entity(new GridPosition(1, 0), new Invisible()))
                .AddTag("Pushable")
                .AddTag("Solid")
            ;

        var pushable2 = _world.AddEntity(new Entity(new GridPosition(2, 0), new Invisible()))
                .AddTag("Pushable")
                .AddTag("Solid")
            ;

        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);

        // nothing moved
        Assert.Equivalent(new GridPosition(0, 0), pusher.Position);
        Assert.Equivalent(new GridPosition(1, 0), pushable.Position);
        Assert.Equivalent(new GridPosition(2, 0), pushable2.Position);
    }
    
    [Fact]
    public void ChainPush_AllMove()
    {
        var pusher = _world.AddEntity(new Entity(new GridPosition(0, 0), new Invisible()))
                .AddTag("Pusher")
                .AddTag("Solid")
            ;

        var pushable = _world.AddEntity(new Entity(new GridPosition(1, 0), new Invisible()))
                .AddTag("Pushable")
                .AddTag("Pusher")
                .AddTag("Solid")
            ;

        var pushable2 = _world.AddEntity(new Entity(new GridPosition(2, 0), new Invisible()))
                .AddTag("Pushable")
                .AddTag("Solid")
            ;

        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);

        // nothing moved
        Assert.Equivalent(new GridPosition(1, 0), pusher.Position);
        Assert.Equivalent(new GridPosition(2, 0), pushable.Position);
        Assert.Equivalent(new GridPosition(3, 0), pushable2.Position);
    }
}
