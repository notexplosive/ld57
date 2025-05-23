using ExplogineMonoGame.Data;
using LD57.Gameplay;
using LD57.Rendering;

namespace TestLD57;

public class GameplayTests
{
    private readonly World _world;

    public GameplayTests()
    {
        _world = new World(new GridPosition(10, 10), new WorldTemplate());
        _world.MoveCompleted += (data, status) => { _world.UpdateEntityList(); };
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
    public void PushIntoWater_Float()
    {
        var pusher = CreateEntity(new GridPosition(0, 0), ["Pusher", "Solid"]);
        var pushable = CreateEntity(new GridPosition(1, 0), ["Pushable", "Solid", "FillsWater", "FloatsInWater"]);
        var water = CreateEntity(new GridPosition(2, 0), ["Water"]);

        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);
        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);

        Assert.True(_world.IsDestroyed(water));
        Assert.False(pushable.IsActive);
        Assert.Equivalent(new GridPosition(2, 0), pusher.Position);
        Assert.Equivalent(new GridPosition(2, 0), pushable.Position);
    }

    [Fact]
    public void PushIntoWater_DoesNotFloat()
    {
        var pusher = CreateEntity(new GridPosition(0, 0), ["Pusher", "Solid"]);
        var pushable = CreateEntity(new GridPosition(1, 0), ["Pushable", "Solid", "FillsWater"]);
        var water = CreateEntity(new GridPosition(2, 0), ["Water"]);

        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);
        _world.Rules.AttemptMoveInDirection(pusher, Direction.Right);

        Assert.True(_world.IsDestroyed(pushable));
        Assert.True(_world.IsDestroyed(water));
        Assert.Equivalent(new GridPosition(2, 0), pusher.Position);
    }

    private Entity CreateEntity(GridPosition position, List<string> tags, List<StateKeyValue>? statePairs = null)
    {
        var template = new EntityTemplate
        {
            Tags = tags,
            State = ConvertToDictionary(statePairs),
            TemplateName = "Fake Template",
            Frame = 0,
            SpriteSheetName = null
        };

        return _world.AddEntity(_world.CreateEntityFromTemplate(template, position, new Dictionary<string, string>()));
    }

    private Dictionary<string, string> ConvertToDictionary(List<StateKeyValue>? statePairs)
    {
        var result = new Dictionary<string, string>();
        if (statePairs == null)
        {
            return result;
        }

        foreach (var statePair in statePairs)
        {
            result[statePair.Key] = statePair.Value;
        }

        return result;
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

        MoveEntity(pusher, Direction.Right);

        // nothing moved
        Assert.Equivalent(new GridPosition(1, 0), pusher.Position);
        Assert.Equivalent(new GridPosition(2, 0), pushable.Position);
        Assert.Equivalent(new GridPosition(3, 0), pushable2.Position);
    }

    private void MoveEntity(Entity mover, Direction direction)
    {
        _world.Rules.AttemptMoveInDirection(mover, direction);
    }

    private void WarpEntity(Entity mover, GridPosition position)
    {
        _world.Rules.WarpToPosition(mover, position);
    }

    private void AssertPassable(GridPosition gridPosition, List<string> tags, bool shouldPass)
    {
        var horizontal = CreateEntity(gridPosition - new GridPosition(1, 0), tags);
        MoveEntity(horizontal, Direction.Right);
        var canPass = horizontal.Position == gridPosition;

        _world.Destroy(horizontal);
        _world.UpdateEntityList();

        if (shouldPass)
        {
            Assert.True(canPass);
        }
        else
        {
            Assert.False(canPass);
        }
    }

    [Fact]
    public void Walls_Are_Walls()
    {
        var wall = CreateEntity(new GridPosition(0, 0), ["Solid"]);

        AssertPassable(new GridPosition(0, 0), ["Solid"], false);
    }

    [Fact]
    public void Door_OneButton()
    {
        var walker = CreateEntity(new GridPosition(0, 0), ["PressesButtons"]);
        var button = CreateEntity(new GridPosition(1, 0), ["Button", "Signal"]);
        var door = CreateEntity(new GridPosition(5, 5), ["Door", "Signal"]);

        AssertPassable(door.Position, ["Solid"], false);
        MoveEntity(walker, Direction.Right);
        AssertPassable(door.Position, ["Solid"], true);
    }

    [Fact]
    public void Door_TwoButtons()
    {
        var walker1 = CreateEntity(new GridPosition(0, 0), ["PressesButtons"]);
        var button = CreateEntity(new GridPosition(1, 0), ["Button", "Signal"]);
        var button2 = CreateEntity(new GridPosition(1, 1), ["Button", "Signal"]);
        var walker2 = CreateEntity(new GridPosition(0, 1), ["PressesButtons"]);
        var door = CreateEntity(new GridPosition(5, 5), ["Door", "Signal"]);

        AssertPassable(door.Position, ["Solid"], false);
        MoveEntity(walker1, Direction.Right);
        AssertPassable(door.Position, ["Solid"], false);
        MoveEntity(walker2, Direction.Right);
        AssertPassable(door.Position, ["Solid"], true);
    }

    [Fact]
    public void Door_AlreadyPressed()
    {
        var presser = CreateEntity(new GridPosition(0, 0), ["PressesButtons"]);
        var button = CreateEntity(new GridPosition(0, 0), ["Button", "Signal"]);
        var door = CreateEntity(new GridPosition(5, 5), ["Door", "Signal"]);

        AssertPassable(door.Position, ["Solid"], true);
    }

    [Fact]
    public void Door_Inverted()
    {
        var presser = CreateEntity(new GridPosition(0, 0), ["PressesButtons"]);
        var button = CreateEntity(new GridPosition(0, 0), ["Button", "Signal"]);
        var door = CreateEntity(new GridPosition(5, 5), ["Door", "Signal"], [new StateKeyValue("inverted", "true")]);

        AssertPassable(door.Position, ["Solid"], true);
        MoveEntity(presser, Direction.Right);
        AssertPassable(door.Position, ["Solid"], false);
    }

    [Fact]
    public void Door_NoButtonsMeansClosed()
    {
        var door =
            CreateEntity(new GridPosition(5, 5), ["Door", "Signal"], [new StateKeyValue("channel", "0")]);

        AssertPassable(door.Position, ["Solid"], false);
    }

    [Fact]
    public void Door_Overlapping()
    {
        var position = new GridPosition(5, 5);
        var open =
            CreateEntity(position, ["Door", "Signal"], [new StateKeyValue("channel", "0")]);
        var closed =
            CreateEntity(position, ["Door", "Signal"], [new StateKeyValue("channel", "1")]);
        var presser =
            CreateEntity(new GridPosition(0, 0), ["PressesButtons"]);
        var buttonInFirstRoom =
            CreateEntity(new GridPosition(0, 0), ["Button", "Signal"], [new StateKeyValue("channel", "0")]);

        AssertPassable(position, ["Solid"], false);
    }

    [Fact]
    public void Door_OpenThenChangeRoom()
    {
        var presser =
            CreateEntity(new GridPosition(0, 0), ["PressesButtons"]);

        var buttonInFirstRoom =
            CreateEntity(new GridPosition(1, 0), ["Button", "Signal"], [new StateKeyValue("channel", "0")]);
        var buttonInOtherRoom =
            CreateEntity(new GridPosition(-4, -4), ["Button", "Signal"], [new StateKeyValue("channel", "0")]);

        var room1ToOpen =
            CreateEntity(new GridPosition(5, 5), ["Door", "Signal"], [new StateKeyValue("channel", "0")]);
        var room1Closed =
            CreateEntity(new GridPosition(6, 6), ["Door", "Signal"], [new StateKeyValue("channel", "1")]);
        var room2ToOpen =
            CreateEntity(new GridPosition(-5, -5), ["Door", "Signal"], [new StateKeyValue("channel", "0")]);
        var room2Closed =
            CreateEntity(new GridPosition(-2, -2), ["Door", "Signal"], [new StateKeyValue("channel", "1")]);

        var otherRoom = _world.GetRoomAt(room2ToOpen.Position);
        Assert.True(_world.CurrentRoom.TopLeft != otherRoom.TopLeft);

        // everything is closed
        AssertPassable(room1ToOpen.Position, ["Solid"], false);
        AssertPassable(room1Closed.Position, ["Solid"], false);
        AssertPassable(room2ToOpen.Position, ["Solid"], false);
        AssertPassable(room2Closed.Position, ["Solid"], false);

        WarpEntity(presser, buttonInFirstRoom.Position);

        // open door in current room
        AssertPassable(room1ToOpen.Position, ["Solid"], true);
        AssertPassable(room1Closed.Position, ["Solid"], false);
        AssertPassable(room2ToOpen.Position, ["Solid"], false);
        AssertPassable(room2Closed.Position, ["Solid"], false);

        WarpEntity(presser, buttonInOtherRoom.Position);

        // close door in current room, open door in other room (even though it's not the current room)
        AssertPassable(room1ToOpen.Position, ["Solid"], false);
        AssertPassable(room1Closed.Position, ["Solid"], false);
        AssertPassable(room2ToOpen.Position, ["Solid"], true);
        AssertPassable(room2Closed.Position, ["Solid"], false);

        _world.SetCurrentRoom(otherRoom);

        // nothing changed, we're agnostic to the current room
        AssertPassable(room1ToOpen.Position, ["Solid"], false);
        AssertPassable(room1Closed.Position, ["Solid"], false);
        AssertPassable(room2ToOpen.Position, ["Solid"], true);
        AssertPassable(room2Closed.Position, ["Solid"], false);
    }

    [Fact]
    public void Door_Airlock()
    {
        var solidPresser = CreateEntity(new GridPosition(0, 0), ["PressesButtons", "Solid"]);
        var button = CreateEntity(new GridPosition(1, 0), ["Button", "Signal"]);
        var door = CreateEntity(new GridPosition(2, 0), ["Door", "Signal"]);

        MoveEntity(solidPresser, Direction.Right); // press button
        MoveEntity(solidPresser, Direction.Right); // walk onto door (is now closed)
        MoveEntity(solidPresser, Direction.Right); // walk off of door
        MoveEntity(solidPresser, Direction.Left); // walk back towards door, can't go anymore

        Assert.Equivalent(new GridPosition(3, 0), solidPresser.Position);
    }

    [Fact]
    public void Item_Xyzzy()
    {
        var spawnPosition = new GridPosition(5, 5);
        var firstUsePosition = new GridPosition(2, 3);
        var secondUsePosition = new GridPosition(7, 2);
        var thirdUsePosition = new GridPosition(-300, -200);
        var user = CreateEntity(spawnPosition, [], []);
        var anchor = new AnchorItemBehavior();

        WarpEntity(user, firstUsePosition);
        UseItem(user, anchor);
        Assert.Equivalent(firstUsePosition, user.Position);

        WarpEntity(user, secondUsePosition);
        UseItem(user, anchor);
        Assert.Equivalent(firstUsePosition, user.Position);

        WarpEntity(user, thirdUsePosition);
        UseItem(user, anchor);
        Assert.Equivalent(secondUsePosition, user.Position);

        UseItem(user, anchor);
        Assert.Equivalent(thirdUsePosition, user.Position);
    }

    [Fact]
    public void Item_Capture_Basic()
    {
        var item = new CaptureGloveItemBehavior();
        var user = CreateEntity(new GridPosition(0, 0), [], []);
        user.MostRecentMoveDirection = Direction.Right;
        var thingToGrab = CreateEntity(new GridPosition(1, 0), ["Capturable"], []);

        UseItem(user, item);

        Assert.True(_world.IsDestroyed(thingToGrab));

        WarpEntity(user, new GridPosition(5, 5));
        MoveEntity(user, Direction.Left);

        UseItem(user, item);

        Assert.False(_world.IsDestroyed(thingToGrab));
        Assert.Equivalent(new GridPosition(3, 5), thingToGrab.Position);

        MoveEntity(user, Direction.Right);
        UseItem(user, item);

        // grabbed thing is unaffected
        Assert.False(_world.IsDestroyed(thingToGrab));
        Assert.Equivalent(new GridPosition(3, 5), thingToGrab.Position);
    }

    [Fact]
    public void Item_Capture_DropIntoWater()
    {
        var item = new CaptureGloveItemBehavior();
        var user = CreateEntity(new GridPosition(0, 0), [], []);
        user.MostRecentMoveDirection = Direction.Right;
        var thingToGrab = CreateEntity(new GridPosition(1, 0), ["Capturable", "FillsWater"], []);
        var water = CreateEntity(new GridPosition(5, 5), ["Water"], []);

        UseItem(user, item);

        WarpEntity(user, new GridPosition(4, 5));
        MoveEntity(user, Direction.Right);

        UseItem(user, item);
        
        _world.UpdateEntityList();

        Assert.True(_world.IsDestroyed(water));
        AssertPassable(new GridPosition(5, 5), ["Solid"], true);
    }
    
    [Fact]
    public void Item_Capture_CantCapture()
    {
        // todo: attempt to drop into a solid

        var item = new CaptureGloveItemBehavior();
        var user = CreateEntity(new GridPosition(0, 0), [], []);
        user.MostRecentMoveDirection = Direction.Right;
        var thingToGrab = CreateEntity(new GridPosition(1, 0), [], []);

        UseItem(user, item);

        Assert.False(_world.IsDestroyed(thingToGrab));
    }

    [Fact]
    public void Item_Capture_CantDrop()
    {
        var item = new CaptureGloveItemBehavior();
        var user = CreateEntity(new GridPosition(0, 0), [], []);
        user.MostRecentMoveDirection = Direction.Right;
        var thingToGrab = CreateEntity(new GridPosition(1, 0), ["Capturable", "Solid"], []);
        var blocker = CreateEntity(new GridPosition(5, 5), ["Solid"], []);

        UseItem(user, item);
        
        WarpEntity(user, blocker.Position - new GridPosition(1,0));
        
        UseItem(user, item);

        Assert.True(_world.IsDestroyed(thingToGrab));
    }
    
    [Fact]
    public void Item_Capture_OverridesTile()
    {
        var item = new CaptureGloveItemBehavior();
        var user = CreateEntity(new GridPosition(0, 0), [], []);
        user.MostRecentMoveDirection = Direction.Right;
        var thingToGrab = CreateEntity(new GridPosition(1, 0), ["Capturable", "ClearOnDrop"], []);
        var blocker = CreateEntity(new GridPosition(5, 5), ["Solid"], []);

        UseItem(user, item);
        
        WarpEntity(user, blocker.Position - new GridPosition(1,0));
        
        UseItem(user, item);

        AssertPassable(blocker.Position, ["Solid"], true);
    }

    private void UseItem(Entity user, ItemBehavior item)
    {
        item.Execute(_world, user);
    }

    private readonly record struct StateKeyValue(string Key, string Value);
}
