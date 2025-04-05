using LD57.Gameplay;

namespace LD57.Rules;

public class CameraFollowsEntity : IGameRule
{
    private readonly Entity _entityToFollow;

    public void OnMoveCompleted(World world, MoveData moveData)
    {
        if (!world.CurrentRoom.Contains(moveData.NewPosition))
        {
            world.CurrentRoom = world.GetRoomAt(_entityToFollow.Position);
        }
    }

    public bool ShouldInterruptMove(World world, MoveData moveData)
    {
        return false;
    }

    public CameraFollowsEntity(Entity entityToFollow)
    {
        _entityToFollow = entityToFollow;
    }
}
