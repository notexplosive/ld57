using ExplogineMonoGame;
using LD57.CartridgeManagement;
using LD57.Gameplay.Triggers;

namespace LD57.Gameplay.Behaviors;

public class ChangeTileFrameBasedOnState : IEntityBehavior
{
    private readonly string _openKey;
    private readonly string _openFrameKey;
    private readonly string _closedFrameKey;

    public ChangeTileFrameBasedOnState(string openKey, string openFrameKey, string closedFrameKey)
    {
        _openKey = openKey;
        _openFrameKey = openFrameKey;
        _closedFrameKey = closedFrameKey;
    }
    
    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is not StateChangeTrigger stateChangeTrigger)
        {
            return;
        }

        if (stateChangeTrigger.Key != _openKey)
        {
            return;
        }

        var isOpen = self.State.GetBool(_openKey) == true;
        var sheet = self.State.GetString("sheet") ?? "Entities";

        if (!self.Appearance.TileState.HasValue)
        {
            return;
        }

        var openFrame = self.State.GetInt(_openFrameKey) ?? 0;
        var closedFrame = self.State.GetInt(_closedFrameKey) ?? 0;

        var frame = isOpen ? openFrame : closedFrame;

        self.Appearance.TileState = self.Appearance.TileState.Value with
        {
            Frame = frame, SpriteSheet = LdResourceAssets.Instance.Sheets[sheet]
        };
    }
}
