using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Rules;

public class CameraFollowsEntity : IGameRule
{
    private readonly Entity _entityToFollow;

    public CameraFollowsEntity(World world, Entity entityToFollow)
    {
        _entityToFollow = entityToFollow;
        UpdateCamera(world, entityToFollow.Position);
    }

    public void OnMoveCompleted(World world, MoveData moveData)
    {
        var newPosition = moveData.Destination;
        UpdateCamera(world, newPosition);
    }

    private void UpdateCamera(World world, GridPosition newPosition)
    {
        if (!world.CurrentRoom.Contains(newPosition))
        {
            world.SetCurrentRoom(world.GetRoomAt(_entityToFollow.Position));
        }
    }

    public bool ShouldInterruptMove(World world, MoveData moveData)
    {
        return false;
    }
}
