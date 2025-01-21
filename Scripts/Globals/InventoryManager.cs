using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Registry;
using Game.Resources;
using Game.Utils.Json.Models;
using Godot;

namespace Game.Globals;

public partial class InventoryManager : Global<InventoryManager>
{
    [Signal] public delegate void UpdatedEventHandler(ItemGroup item);

    public static event UpdatedEventHandler InventoryUpdated
    {
        add => Instance.Updated += value;
        remove => Instance.Updated -= value;
    }

    private readonly ReadOnlyDictionary<Item.Category, List<ItemGroup>> Inventory = new(
        new Dictionary<Item.Category, List<ItemGroup>>
        {
            { Item.Category.Weapon, [] },
            { Item.Category.Quest, [] },
            { Item.Category.Consumable, [] },
            { Item.Category.Material, [] }
        }
    );

    public override void _Ready()
    {
        // var group = new ItemGroup();
        // group.Item = GD.Load<Item>("res://resources/items/apple.tres");
        // group.Quantity = 5;
        //
        // AddItem(group);
        //
        // group = new ItemGroup();
        // group.Item = GD.Load<Item>("res://resources/items/rock.tres");
        // group.Quantity = 10;
        //
        // AddItem(group);
        //
        // group = new ItemGroup();
        // group.Item = GD.Load<Weapon>("res://resources/weapons/swords/sword.tres");
        // group.Quantity = 1;
        //
        // AddItem(group);
        //
        // group = new ItemGroup();
        // group.Item = GD.Load<Weapon>("res://resources/weapons/daggers/dagger.tres");
        // group.Quantity = 1;
        //
        // AddItem(group);

        var inventory = SaveManager.InventoryData;

        inventory.Items.ForEach(item =>
        {
            AddItem(new ItemGroup
            {
                Item = ItemRegistry.Get(item.UniqueName),
                Quantity = item.Amount
            });
        });
    }

    public static void AddItem(ItemGroup group)
    {
        var itemGroup = Instance.Inventory[group.Item.ItemCategory]
            .Find(g => g.Item.UniqueName == group.Item.UniqueName);

        if (itemGroup is not null)
            itemGroup.Quantity += group.Quantity;
        else
            Instance.Inventory[group.Item.ItemCategory].Add(group);

        Instance.EmitSignal(SignalName.Updated, itemGroup ?? group);
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

    public static InventoryData ToData() => new()
    {
        Items = Instance.Inventory.SelectMany(i => i.Value).Select(item => new InventoryData.Item
        {
            UniqueName = item.Item.UniqueName,
            Amount = item.Quantity
        }).ToList()
    };
}