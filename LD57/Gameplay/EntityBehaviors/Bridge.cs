using LD57.Gameplay.Triggers;
using LD57.Rendering;

namespace LD57.Gameplay.EntityBehaviors;

public class Bridge : IEntityBehavior
{
    private readonly string _stateKey;
    private readonly bool _targetValue;
    private readonly EntityTemplate _template;
    private Entity? _createdEntity;
    private TileState? _savedTileState;

    public Bridge(string stateKey, bool targetValue, string templateName)
    {
        _stateKey = stateKey;
        _targetValue = targetValue;
        _template = ResourceAlias.EntityTemplate(templateName) ?? new EntityTemplate();
    }

    public void OnTrigger(Entity self, IBehaviorTrigger trigger)
    {
        if (trigger is not StateChangeTrigger stateChangeTrigger)
        {
            return;
        }

        if (stateChangeTrigger.Key != _stateKey)
        {
            return;
        }

        
        if (_createdEntity == null)
        {
            // This creates the entity but does not add it to the world
            _createdEntity = self.World.CreateEntityFromTemplate(_template, self.Position, []);
        }

        if (_savedTileState == null)
        {
            _savedTileState = self.Appearance.TileState;
        }
        
        var shouldBeSubmerged = self.State.GetBoolOrFallback(_stateKey, false) == _targetValue;
        var createdEntityIsDestroyed = _createdEntity.World.IsDestroyed(_createdEntity);

        if (shouldBeSubmerged)
        {
            // todo: this should move to a different behavior
            self.Appearance.TileState = TileState.BackgroundOnly(_savedTileState.Value.ForegroundColor, self.State.GetFloatOrFallback("underwater_intensity", 0.25f));
            self.SetOverrideSortPriority(50);
            
            if (createdEntityIsDestroyed)
            {
                self.World.AddEntity(_createdEntity);
            }
        }
        else
        {
            self.Appearance.TileState = _savedTileState.Value;
            self.World.Destroy(_createdEntity);
            self.ClearOverridenSortPriority();
        }
        
        self.State.Set("is_risen", !shouldBeSubmerged);
        self.World.Rules.DoUpdateAtPosition(self.Position);
    }
}

