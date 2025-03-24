using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Autoload;
using Game.Data;
using Godot;
using Item = Game.Data.Item;
using Game.Entities;

namespace Game.Components;

[GlobalClass, Icon("res://assets/icons/inventory-manager.svg")]
public partial class InventoryManager : Node
{
    [Signal] public delegate void UpdatedEventHandler(ItemGroup item);
    [Signal] public delegate void PickupEventHandler(ItemGroup item);
    [Signal] public delegate void RemoveEventHandler(ItemGroup item);

    private readonly ReadOnlyDictionary<Item.Category, List<ItemGroup>> Inventory = new(
        new Dictionary<Item.Category, List<ItemGroup>>
        {
            { Item.Category.Weapon, [] },
            { Item.Category.Quest, [] },
            { Item.Category.Consumable, [] },
            { Item.Category.Material, [] }
        }
    );

    public void AddItem(ItemGroup group)
    {
        var itemGroup = Inventory[group.Item.ItemCategory]
            .Find(g => g.Item.Id == group.Item.Id);

        if (itemGroup is not null)
            itemGroup.Quantity += group.Quantity;
        else
            Inventory[group.Item.ItemCategory].Add(group);

        EmitSignalPickup(itemGroup ?? group);
        EmitSignalUpdated(itemGroup ?? group);
        Log.Debug($"Added {group} to the inventory.");
    }

    public void RemoveItem(ItemGroup group)
    {
        var itemGroup = Inventory[group.Item.ItemCategory]
            .Find(g => g.Item.Id == group.Item.Id);

        if (itemGroup is null) return;

        itemGroup.Quantity -= group.Quantity;

        if (itemGroup.Quantity <= 0)
            Inventory[group.Item.ItemCategory].Remove(itemGroup);

        EmitSignalRemove(itemGroup);
        EmitSignalUpdated(itemGroup);
        Log.Debug($"Removed {group} from the inventory.");
    }

    public ItemGroup GetItem(Item item) => Inventory[item.ItemCategory]
        .Find(g => g.Item.Id == item.Id);

    public IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) =>
        Inventory[category].AsReadOnly();

    public bool HasItem(ItemGroup group) => Inventory[group.Item.ItemCategory]
        .Any(g => g.Item.Id == group.Item.Id && g.Quantity >= group.Quantity);

    public bool HasItem(Item item) => Inventory[item.ItemCategory]
        .Any(g => g.Item.Id == item.Id);

    public void Clear()
    {
        foreach (var key in Inventory.Keys)
        {
            var items = Inventory[key].ToList();

            items.ForEach(RemoveItem);
        }
    }

    public List<Game.Common.Models.Item> GetItemsAsModel() => Inventory.Values
        .SelectMany(i => i)
        .Select(i => new Game.Common.Models.Item
        {
            Id = i.Item.Id,
            Amount = i.Quantity
        })
        .ToList();
}