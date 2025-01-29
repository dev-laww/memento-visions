using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Exceptions.Command;
using Game.Registry;
using Game.Resources;
using Game.Utils;
using Game.Utils.Json.Models;
using Godot;

namespace Game.Globals;

public partial class InventoryManager : Global<InventoryManager>
{
    public delegate void UpdatedEventHandler(ItemGroup item);
    public delegate void PickupEventHandler(ItemGroup item);
    public delegate void RemoveEventHandler(ItemGroup item);

    public static event UpdatedEventHandler Updated;
    public static event PickupEventHandler Pickup;
    public static event RemoveEventHandler Remove;


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

        CommandInterpreter.Register("add item", AddItemCommand,
            "Adds an item to the inventory. Usage: add item [uniqueName] [quantity]");
        CommandInterpreter.Register("remove item", RemoveItemCommand,
            "Removes an item from the inventory. Usage: remove item [uniqueName] [quantity]");
        CommandInterpreter.Register("clear inventory", ClearInventory, "Clears the inventory.");
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        CommandInterpreter.Unregister("add item");
        CommandInterpreter.Unregister("remove item");
        CommandInterpreter.Unregister("clear inventory");
    }

    public override void _Ready()
    {
        var inventory = SaveManager.InventoryData;

        Log.Info("Loading inventory...");
        inventory.Items.ForEach(item =>
        {
            AddItem(new ItemGroup
            {
                Item = ItemRegistry.Get(item.Id),
                Quantity = item.Amount
            });
        });
    }

    public static void AddItem(ItemGroup group)
    {
        var itemGroup = Instance.Inventory[group.Item.ItemCategory]
            .Find(g => g.Item.Id == group.Item.Id);

        if (itemGroup is not null)
            itemGroup.Quantity += group.Quantity;
        else
            Instance.Inventory[group.Item.ItemCategory].Add(group);

        Updated?.Invoke(itemGroup ?? group);
        Pickup?.Invoke(group);
        Log.Debug($"Added {group} to the inventory.");
    }

    public static void RemoveItem(ItemGroup group)
    {
        var itemGroup = Instance.Inventory[group.Item.ItemCategory]
            .Find(g => g.Item.Id == group.Item.Id);

        if (itemGroup is null) return;

        itemGroup.Quantity -= group.Quantity;

        if (itemGroup.Quantity <= 0)
            Instance.Inventory[group.Item.ItemCategory].Remove(itemGroup);

        Remove?.Invoke(itemGroup);
        Updated?.Invoke(itemGroup);
        Log.Debug($"Removed {group} from the inventory.");
    }

    public static IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) =>
        Instance.Inventory[category].AsReadOnly();

    public static bool HasItem(ItemGroup group) =>
        Instance.Inventory[group.Item.ItemCategory]
            .Any(g => g.Item.Id == group.Item.Id && g.Quantity >= group.Quantity);

    public static InventoryData ToData() => new()
    {
        Items = Instance.Inventory.SelectMany(i => i.Value).Select(item => new InventoryData.Item
        {
            Id = item.Item.Id,
            Amount = item.Quantity
        }).ToList()
    };

    private static void AddItemCommand(string uniqueName, int quantity = 1)
    {
        var item = ItemRegistry.Get(uniqueName) ?? throw new CommandException($"Item '{uniqueName}' not found.");

        AddItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    private static void RemoveItemCommand(string uniqueName, int quantity = 1)
    {
        var item = ItemRegistry.Get(uniqueName) ?? throw new CommandException($"Item '{uniqueName}' not found.");

        RemoveItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    private static void ClearInventory()
    {
        foreach (var category in Instance.Inventory.Keys)
        {
            var items = Instance.Inventory[category].ToList();

            items.ForEach(RemoveItem);
        }
    }
}