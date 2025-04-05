using ExplogineMonoGame.Data;
using LD57.Gameplay;

namespace LD57.Rules;

public class PushPushables : GameRule
{
    public override void OnMoveCompleted(World world, MoveData moveData)
    {
        
    }

    public override bool ShouldInterruptMove(World world, MoveData moveData)
    {
        if (!moveData.Mover.HasTag("Pusher") || moveData.Direction == Direction.None)
        {
            return false;
        }

        var wasInterrupted = false;
        foreach (var entity in world.GetEntitiesAt(moveData.NewPosition))
        {
            if (entity.HasTag("Pushable"))
            {
                var status = world.Rules.AttemptMoveInDirection(entity, moveData.Direction);

                if (status.WasInterrupted)
                {
                    wasInterrupted = true;
                }
            }
        }

        return wasInterrupted;
    }
}
