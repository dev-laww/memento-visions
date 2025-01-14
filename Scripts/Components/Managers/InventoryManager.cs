using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Resources;
using Godot;

namespace Game.Components.Managers;

public partial class InventoryManager : Node
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

    public void AddItem(Item item, int quantity = 1)
    {
        var itemGroup = Inventory[item.ItemCategory].Find(group => group.Item.UniqueName == item.UniqueName);

        if (itemGroup is not null)
            itemGroup.Quantity += quantity;
        else
        {
            itemGroup = new ItemGroup { Item = item, Quantity = quantity };
            Inventory[item.ItemCategory].Add(itemGroup);
        }

        EmitSignal(SignalName.Updated, itemGroup);
    }

    public void RemoveItem(Item item, int quantity = 1)
    {
        var itemGroup = Inventory[item.ItemCategory].Find(group => group.Item.UniqueName == item.UniqueName);

        if (itemGroup is null) return;

        itemGroup.Quantity -= quantity;

        if (itemGroup.Quantity <= 0)
            Inventory[item.ItemCategory].Remove(itemGroup);

        EmitSignal(SignalName.Updated, itemGroup);
    }

    public IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) => Inventory[category].AsReadOnly();
}