using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Resources;
using Godot;

namespace Game.Globals;

public partial class PlayerInventoryManager : Global<PlayerInventoryManager>
{
    [Signal] public delegate void UpdatedEventHandler(ItemGroup item);

    private readonly ReadOnlyDictionary<Item.Category, List<ItemGroup>> Inventory = new(
        new Dictionary<Item.Category, List<ItemGroup>>
        {
            { Item.Category.Weapon, [] },
            { Item.Category.Quest, [] },
            { Item.Category.Consumable, [] },
            { Item.Category.Material, [] }
        }
    );

    public static void AddItem(ItemGroup group)
    {
        var itemGroup = Instance.Inventory[group.Item.ItemCategory]
            .Find(g => g.Item.UniqueName == group.Item.UniqueName);

        if (itemGroup is not null)
            itemGroup.Quantity += group.Quantity;
        else
        {
            Instance.Inventory[group.Item.ItemCategory].Add(group);
        }

        Instance.EmitSignal(SignalName.Updated, itemGroup);
    }

    public static void RemoveItem(ItemGroup group)
    {
        var itemGroup = Instance.Inventory[group.Item.ItemCategory]
            .Find(g => g.Item.UniqueName == group.Item.UniqueName);

        if (itemGroup is null) return;

        itemGroup.Quantity -= group.Quantity;

        if (itemGroup.Quantity <= 0)
            Instance.Inventory[group.Item.ItemCategory].Remove(itemGroup);

        Instance.EmitSignal(SignalName.Updated, itemGroup);
    }

    public static IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) =>
        Instance.Inventory[category].AsReadOnly();

    public static bool HasItem(ItemGroup group) =>
        Instance.Inventory[group.Item.ItemCategory]
            .Any(g => g.Item.UniqueName == group.Item.UniqueName && g.Quantity >= group.Quantity);
}