using System.Collections.Generic;
using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public class Inventory
{
    private readonly Dictionary<ActionButton, ItemBehavior> _itemBehaviors = new();
    private readonly Dictionary<ActionButton, Entity> _itemEntities = new();
    private bool _canReset;
    public bool IsPlayingItemAnimation { get; private set; }

    public void EnableReset()
    {
        _canReset = true;
    }

    public void AnimateUse(ActionButton slot, World world, Entity user, SequenceTween tween)
    {
        if (HasSomethingInSlot(slot))
        {
            tween
                .Add(new CallbackTween(() => { IsPlayingItemAnimation = true; }))
                .Add(GetBehaviorInSlot(slot).PlayAnimation(world, user))
                .Add(new CallbackTween(() => { IsPlayingItemAnimation = false; }))
                ;
        }
    }

    public void InstantUse(ActionButton slot, World world, Entity user)
    {
        GetBehaviorInSlot(slot).Execute(world, user);
    }

    private ItemBehavior GetBehaviorInSlot(ActionButton slot)
    {
        if (_itemBehaviors.TryGetValue(slot, out var result))
        {
            return result;
        }

        return new EmptyItemBehavior();
    }

    public void PaintWorldOverlay(ActionButton button, AsciiScreen screen, World world, Entity user, float dt)
    {
        GetBehaviorInSlot(button).PaintInWorld(screen, world, user, dt);
    }

    public void Equip(ActionButton slot, Entity entity)
    {
        _itemEntities[slot] = entity;
        var itemBehavior = CreateBehavior(slot, GetBehaviorName(_itemEntities[slot]));
        itemBehavior.SetUiTile(entity.TileState);

        _itemBehaviors[slot] = itemBehavior;
    }

    /// <summary>
    ///     For unit tests
    /// </summary>
    public void EquipBehaviorDirectly(ActionButton slot, string itemBehavior)
    {
        _itemBehaviors[slot] = CreateBehavior(slot, itemBehavior);
    }

    private ItemBehavior CreateBehavior(ActionButton slot, string behaviorName)
    {
        if (behaviorName == "hook")
        {
            return new HookItemBehavior();
        }

        if (behaviorName == "swap")
        {
            return new SwapItemBehavior();
        }

        if (behaviorName == "xyzzy")
        {
            return new AnchorItemBehavior();
        }

        if (behaviorName == "glove")
        {
            return new CaptureGloveItemBehavior();
        }

        return new EmptyItemBehavior();
    }

    public bool HasSomethingInSlot(ActionButton slot)
    {
        return _itemEntities.ContainsKey(slot);
    }

    public Entity? GetEntityInSlot(ActionButton slot)
    {
        return _itemEntities.GetValueOrDefault(slot);
    }

    public Entity RemoveFromSlot(ActionButton slot, World world, Entity player)
    {
        var entity = _itemEntities[slot];

        entity.Position = player.Position;
        world.AddEntity(entity);

        GetBehaviorInSlot(slot).OnRemove(world, player);

        _itemEntities.Remove(slot);
        _itemBehaviors.Remove(slot);
        return entity;
    }

    public static string GetHumanReadableName(Entity entity)
    {
        return entity.State.GetStringOrDefault("item_name", "Unknown Item");
    }

    public static string GetBehaviorName(Entity entity)
    {
        return entity.State.GetStringOrDefault("item_behavior", "none");
    }

    public void DrawHud(AsciiScreen screen, string? currentZoneName, float dt)
    {
        var bottomHudTopLeft = new GridPosition(0, 19);
        screen.PutFrameRectangle(ResourceAlias.PopupFrame,
            GridRectangle.FromTopLeftAndSize(bottomHudTopLeft, new GridPosition(screen.Width, 2)));
        screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), Status());
        screen.PutString(bottomHudTopLeft + new GridPosition(2, 0), "Z[ ]");
        if (HasSomethingInSlot(ActionButton.Primary))
        {
            var itemBehavior = GetBehaviorInSlot(ActionButton.Primary);
            screen.PutTile(bottomHudTopLeft + new GridPosition(4, 0), itemBehavior.DefaultHudTile,
                itemBehavior.GetTweenableGlyph());
        }

        screen.PutString(bottomHudTopLeft + new GridPosition(7, 0), "X[ ]");
        if (HasSomethingInSlot(ActionButton.Secondary))
        {
            var itemBehavior = GetBehaviorInSlot(ActionButton.Secondary);
            screen.PutTile(bottomHudTopLeft + new GridPosition(9, 0), itemBehavior.DefaultHudTile,
                itemBehavior.GetTweenableGlyph());
        }

        if (_canReset)
        {
            screen.PutString(bottomHudTopLeft + new GridPosition(12, 0), "R[ ]");
            screen.PutTile(bottomHudTopLeft + new GridPosition(14, 0), TileState.Sprite(ResourceAlias.Entities, 27));
        }

        screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), currentZoneName ?? "???");
    }

    private string Status()
    {
        return "";
    }

    public void ClearItems()
    {
        _itemEntities.Clear();
        _itemBehaviors.Clear();
    }
}
