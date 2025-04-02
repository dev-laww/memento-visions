using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Entities;
using Game.Data;
using Godot;
using GodotUtilities;
using GodotUtilities.Logic;
using WorldItem = Game.World.Item;

namespace Game.Components;

[Tool]
[Scene]
[Icon("res://assets/icons/drop-manager.svg")]
public partial class DropManager : Node
{
    private class Drop
    {
        public Item Item;
        public int Min;
        public int Max;
    }

    [Export]
    public ItemDrop[] Drops
    {
        get => drops;
        set
        {
            drops = value;
            NotifyPropertyListChanged();
        }
    }

    [Node] private ResourcePreloader resourcePreloader;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    private readonly LootTable<Drop> lootTable = new();
    private ItemDrop[] drops;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        if (GetParent() is Entity parent)
        {
            parent.Death += SpawnDrops;
        }

        SetupDrops();
    }

    private void SpawnDrops(Entity.DeathInfo info) => SpawnDrops(info.Position);

    // TODO: Balance this
    public void SpawnDrops(Vector2 position)
    {
        var dropItemCount = MathUtil.RNG.RandiRange(0, Drops.Length);
        var droppedItems = new HashSet<Item>();

        Log.Debug($"Dropping {dropItemCount} items");

        for (var i = 0; i < dropItemCount; i++)
        {
            var drop = lootTable.PickItem();

            while (droppedItems.Contains(drop.Item))
            {
                if (droppedItems.Count == Drops.Length) return;

                drop = lootTable.PickItem();
            }

            droppedItems.Add(drop.Item);
            var item = resourcePreloader.InstanceSceneOrNull<WorldItem>("Item");
            item.ItemGroup = new ItemGroup { Item = drop.Item, Quantity = MathUtil.RNG.RandiRange(drop.Min, drop.Max) };
            item.GlobalPosition = new Vector2(
                position.X + MathUtil.RNG.RandfRange(-16, 16),
                position.Y + MathUtil.RNG.RandfRange(-16, 16)
            );

            GameManager.CurrentScene?.CallDeferred("add_child", item);

            Log.Debug($"Dropped {item.ItemGroup}");
        }
    }

    public void SetDrops(IEnumerable<ItemDrop> drops)
    {
        Drops = drops.ToArray();
        SetupDrops();
    }

    private void SetupDrops()
    {
        foreach (var (item, min, max, weight) in Drops)
        {
            var drop = new Drop { Item = item, Min = min, Max = max };

            lootTable.AddItem(drop, weight);
        }
    }
}