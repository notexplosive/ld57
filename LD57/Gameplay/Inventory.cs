using System.Collections.Generic;
using ExTween;
using LD57.Rendering;

namespace LD57.Gameplay;

public class Inventory
{
    private readonly Dictionary<ActionButton, ItemBehavior> _itemBehaviors = new();
    private readonly Dictionary<ActionButton, Entity> _itemEntities = new();
    public bool IsPlayingItemAnimation { get; private set; }

    public void AnimateUse(ActionButton slot, World world, SequenceTween tween)
    {
        if (HasSomethingInSlot(slot))
        {
            tween
                .Add(new CallbackTween(() => { IsPlayingItemAnimation = true; }))
                .Add(GetItemBehavior(slot).PlayAnimation(world))
                .Add(new CallbackTween(() => { IsPlayingItemAnimation = false; }))
                ;
        }
    }

    public void InstantUse(ActionButton slot, World world)
    {
        GetItemBehavior(slot).Execute(world);
    }

    private ItemBehavior GetItemBehavior(ActionButton slot)
    {
        if (_itemBehaviors.TryGetValue(slot, out var result))
        {
            return result;
        }

        return new EmptyItemBehavior();
    }

    public void PaintWorldOverlay(ActionButton button,AsciiScreen screen, World world, Entity player, float dt)
    {
        GetItemBehavior(button).PaintInWorld(screen, world, player, dt);
    }

    public void Equip(ActionButton slot, Entity entity)
    {
        _itemEntities[slot] = entity;
        var itemBehavior = CreateBehavior(slot);
        if (entity.TileState.HasValue)
        {
            itemBehavior.SetUiTile(entity.TileState.Value);
        }

        _itemBehaviors[slot] = itemBehavior;
    }

    private ItemBehavior CreateBehavior(ActionButton slot)
    {
        var behavior = GetBehaviorName(_itemEntities[slot]);

        if (behavior == "hook")
        {
            return new HookItemBehavior();
        }

        if (behavior == "swap")
        {
            return new SwapItemBehavior();
        }

        if (behavior == "xyzzy")
        {
            return new AnchorItemBehavior();
        }

        if (behavior == "glove")
        {
            return new GloveItemBehavior();
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

    public Entity RemoveFromSlot(ActionButton slot)
    {
        var entity = _itemEntities[slot];

        _itemEntities.Remove(slot);
        _itemBehaviors.Remove(slot);

        return entity;
    }

    public static string GetItemName(Entity entity)
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
        screen.PutFrameRectangle(ResourceAlias.PopupFrame, bottomHudTopLeft,
            bottomHudTopLeft + new GridPosition(screen.Width - 1, 2));
        screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), Status());
        screen.PutString(bottomHudTopLeft + new GridPosition(2, 0), "Z[ ]");
        if (HasSomethingInSlot(ActionButton.Primary))
        {
            var itemBehavior = GetItemBehavior(ActionButton.Primary);
            screen.PutTile(bottomHudTopLeft + new GridPosition(4, 0), itemBehavior.DefaultHudTile,
                itemBehavior.GetTweenableGlyph());
        }

        screen.PutString(bottomHudTopLeft + new GridPosition(7, 0), "X[ ]");
        if (HasSomethingInSlot(ActionButton.Secondary))
        {
            var itemBehavior = GetItemBehavior(ActionButton.Secondary);
            screen.PutTile(bottomHudTopLeft + new GridPosition(9, 0), itemBehavior.DefaultHudTile,
                itemBehavior.GetTweenableGlyph());
        }

        screen.PutString(bottomHudTopLeft + new GridPosition(1, 1), currentZoneName ?? "???");
    }

    private string Status()
    {
        return "";
    }
}
