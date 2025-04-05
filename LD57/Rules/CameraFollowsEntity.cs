using LD57.Gameplay;

namespace LD57.Rules;

public class CameraFollowsEntity : IGameRule
{
    private readonly Entity _entityToFollow;

    public void OnMoveCompleted(World world, MoveCompletedData moveCompletedData)
    {
        if (!world.CurrentRoom.Contains(moveCompletedData.NewPosition))
        {
            world.CurrentRoom = world.GetRoomAt(_entityToFollow.Position);
        }
    }

    public CameraFollowsEntity(Entity entityToFollow)
    {
        _entityToFollow = entityToFollow;
    }
}
