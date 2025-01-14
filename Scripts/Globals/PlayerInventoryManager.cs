using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Resources;
using Godot;

namespace Game.Globals;

public partial class PlayerInventoryManager: Global<PlayerInventoryManager>
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

    public static void AddItem(Item item, int quantity = 1)
    {
        var itemGroup = Instance.Inventory[item.ItemCategory].Find(group => group.Item.UniqueName == item.UniqueName);

        if (itemGroup is not null)
            itemGroup.Quantity += quantity;
        else
        {
            itemGroup = new ItemGroup { Item = item, Quantity = quantity };
            Instance.Inventory[item.ItemCategory].Add(itemGroup);
        }

        Instance.EmitSignal(SignalName.Updated, itemGroup);
    }

    public static void RemoveItem(Item item, int quantity = 1)
    {
        var itemGroup = Instance.Inventory[item.ItemCategory].Find(group => group.Item.UniqueName == item.UniqueName);

        if (itemGroup is null) return;

        itemGroup.Quantity -= quantity;

        if (itemGroup.Quantity <= 0)
            Instance.Inventory[item.ItemCategory].Remove(itemGroup);

        Instance.EmitSignal(SignalName.Updated, itemGroup);
    }

    public static IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) => Instance.Inventory[category].AsReadOnly();
    
    public static bool HasItem(Item item, int quantity = 1) =>
        Instance.Inventory[item.ItemCategory].Any(group => group.Item.UniqueName == item.UniqueName && group.Quantity >= quantity);
}