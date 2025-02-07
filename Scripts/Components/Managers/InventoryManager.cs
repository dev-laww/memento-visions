using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Exceptions;
using Game.Globals;
using Game.Registry;
using Game.Resources;
using Godot;
using Item = Game.Resources.Item;

namespace Game.Components.Managers;

[GlobalClass]
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

        CommandInterpreter.Register(
            "give",
            AddItemCommand,
            "Adds an item to the inventory. Usage: give [uniqueName] [quantity]"
        );
        CommandInterpreter.Register(
            "take",
            RemoveItemCommand,
            "Removes an item from the inventory. Usage: take [uniqueName] [quantity]"
        );
        CommandInterpreter.Register(
            "clear inventory",
            ClearInventory,
            "Clears the inventory."
        );
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        CommandInterpreter.Unregister("give");
        CommandInterpreter.Unregister("take");
        CommandInterpreter.Unregister("clear inventory");

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
        Log.Debug($"Added {itemGroup ?? group} to the inventory.");
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

    private void AddItemCommand(string uniqueName, int quantity = 1)
    {
        var item = ItemRegistry.Get(uniqueName) ?? throw new CommandException($"Item '{uniqueName}' not found.");

        AddItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    private void RemoveItemCommand(string uniqueName, int quantity = 1)
    {
        var item = ItemRegistry.Get(uniqueName) ?? throw new CommandException($"Item '{uniqueName}' not found.");

        RemoveItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    private void ClearInventory()
    {
        foreach (var category in Inventory.Keys)
        {
            var items = Inventory[category].ToList();

            items.ForEach(RemoveItem);
        }
    }
}