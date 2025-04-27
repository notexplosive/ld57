using ExplogineMonoGame;
using LD57.CartridgeManagement;
using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class Door : IEntityBehavior
{
    private readonly string _openKey;

    public Door(string openKey)
    {
        _openKey = openKey;
    }
    
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is StateChangeTrigger stateChangeTrigger)
        {
            if (stateChangeTrigger.Key == _openKey)
            {
                var isOpen = self.State.GetBool(_openKey) == true;
                var sheet = self.State.GetString("sheet") ?? "Entities";

                if (self.Appearance?.TileState.HasValue == true)
                {
                    var openFrame = self.State.GetInt("open_frame") ?? 0;
                    var closedFrame = self.State.GetInt("closed_frame") ?? 0;

                    var frame = isOpen ? openFrame : closedFrame;

                    self.Appearance.TileState = self.Appearance.TileState.Value with
                    {
                        Frame = frame, SpriteSheet = LdResourceAssets.Instance.Sheets[sheet]
                    };
                }
            }
        }
    }
}
