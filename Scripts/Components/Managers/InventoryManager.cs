using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Exceptions;
using Game.Autoload;
using Game.Data;
using Godot;
using Item = Game.Data.Item;

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

    public override void _EnterTree()
    {
        base._EnterTree();

        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        CommandInterpreter.Unregister(this);

        var items = Inventory.Values
            .SelectMany(i => i)
            .Select(i => new Game.Common.Models.Item
            {
                Id = i.Item.Id,
                Amount = i.Quantity
            })
            .ToList();

        SaveManager.Data.SetItemsData(items);
    }

    public override void _Ready()
    {
        var items = SaveManager.Data.Items;

        Log.Info("Loading inventory...");
        items.ForEach(item =>
        {
            if (item.Amount <= 0) return;

            AddItem(new ItemGroup
            {
                Item = ItemRegistry.Get(item.Id),
                Quantity = item.Amount
            });
        });
    }

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
        Log.Debug($"Removed {itemGroup} from the inventory.");
    }

    public IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) =>
        Inventory[category].AsReadOnly();

    public bool HasItem(ItemGroup group) => Inventory[group.Item.ItemCategory]
        .Any(g => g.Item.Id == group.Item.Id && g.Quantity >= group.Quantity);

    [Command(Name = "give", Description = "Adds an item to the inventory.")]
    private void AddItemCommand(string id, int quantity = 1)
    {
        var item = ItemRegistry.Get(id) ?? throw new CommandException($"Item '{id}' not found.");

        AddItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    [Command(Name = "take", Description = "Removes an item from the inventory.")]
    private void RemoveItemCommand(string id, int quantity = 1)
    {
        var item = ItemRegistry.Get(id) ?? throw new CommandException($"Item '{id}' not found.");

        RemoveItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    [Command(Name = "clear-inventory", Description = "Clears the inventory.")]
    private void ClearInventory()
    {
        foreach (var category in Inventory.Keys)
        {
            var items = Inventory[category].ToList();

            items.ForEach(RemoveItem);
        }
    }
}