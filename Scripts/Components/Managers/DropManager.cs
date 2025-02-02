using Game.Resources;
using Godot;
using GodotUtilities;
using GodotUtilities.Logic;
using WorldItem = Game.World.Objects.Item;

namespace Game.Components.Managers;

[Tool]
[Scene]
[GlobalClass]
public partial class DropManager : Node
{
    private class Drop
    {
        public Item Item;
        public int Min;
        public int Max;
    }

    [Export] private ItemDrop[] Drops;

    [Node] private ResourcePreloader resourcePreloader;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }


    private readonly LootTable<Drop> lootTable = new();

    public override void _Ready()
    {
        foreach (var (item, min, max, weight) in Drops)
        {
            var drop = new Drop { Item = item, Min = min, Max = max };

            lootTable.AddItem(drop, weight);
        }
    }


    // TODO: Balance this
    public void SpawnDrops(Vector2 position)
    {
        var dropItemCount = MathUtil.RNG.RandiRange(0, Drops.Length);

        for (var i = 0; i < dropItemCount; i++)
        {
            var drop = lootTable.PickItem();

            if (drop is null) continue;

            var item = resourcePreloader.InstanceSceneOrNull<WorldItem>("Item");
            item.ItemGroup = new ItemGroup { Item = drop.Item, Quantity = MathUtil.RNG.RandiRange(drop.Min, drop.Max) };
            item.GlobalPosition = position;

            GameManager.CurrentScene.AddChild(item);
        }
    }
}