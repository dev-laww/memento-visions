using System.Collections.Generic;
using System.CommandLine.IO;
using Game.Common;
using Game.Common.Utilities;
using Game.Components;
using Game.Data;
using Game.Utils.Extensions;
using GodotUtilities;

namespace Game.Autoload;

[Scene]
public partial class PlayerInventoryManager : Autoload<PlayerInventoryManager>
{
    public static event InventoryManager.UpdatedEventHandler Updated
    {
        add => Instance.inventoryManager.Updated += value;
        remove => Instance.inventoryManager.Updated -= value;
    }

    public static event InventoryManager.PickupEventHandler Pickup
    {
        add => Instance.inventoryManager.Pickup += value;
        remove => Instance.inventoryManager.Pickup -= value;
    }

    public static event InventoryManager.RemoveEventHandler Remove
    {
        add => Instance.inventoryManager.Remove += value;
        remove => Instance.inventoryManager.Remove -= value;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    [Node] private InventoryManager inventoryManager;

    public static Item QuickSlotItem { get; private set; }

    public override void _EnterTree()
    {
        base._EnterTree();
        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        SaveManager.Data.SetItemsData(inventoryManager.GetItemsAsModel());
        SaveManager.Data.Player.QuickUse = QuickSlotItem?.Id ?? string.Empty;
        SaveManager.Save();
    }

    public override void _Ready()
    {
        var items = SaveManager.Data.Items;

        inventoryManager.Updated += OnInventoryUpdate;

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

        var quickSlotItem = ItemRegistry.Get(SaveManager.Data.Player.QuickUse);
        SetQuickSlotItem(quickSlotItem);
    }

    public static void AddItem(ItemGroup item) => Instance.inventoryManager.AddItem(item);
    public static void RemoveItem(ItemGroup item) => Instance.inventoryManager.RemoveItem(item);
    public static ItemGroup GetItem(Item item) => item is null ? null : Instance.inventoryManager.GetItem(item);

    public static void UseItem(ItemGroup group)
    {
        if (!HasItem(group))
        {
            Log.Warn($"Item '{group}' not found or not enough quantity.");
            return;
        }

        var item = group.Item;
        var player = Instance.GetPlayer();

        if (player is null)
        {
            Log.Warn("Player not found in scene.");
            return;
        }

        for (var i = 0; i < group.Quantity; i++)
            item.Use(player);

        RemoveItem(group);
        Log.Info($"Used {group}.");
    }

    public static void SetQuickSlotItem(Item item)
    {
        QuickSlotItem = item;

        if (item is null)
        {
            GameEvents.EmitQuickUseSlotUpdated(null);
            return;
        }

        var group = Instance.inventoryManager.GetItem(item);

        GameEvents.EmitQuickUseSlotUpdated(group);
    }

    public static void UseItem(Item item, int quantity = 1) => UseItem(new ItemGroup
    {
        Item = item,
        Quantity = quantity
    });

    public static void UseQuickSlotItem()
    {
        if (QuickSlotItem is null) return;

        UseItem(QuickSlotItem);
    }

    public static IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) =>
        Instance.inventoryManager.GetItemsFromCategory(category);

    public static bool HasItem(ItemGroup group) => Instance.inventoryManager.HasItem(group);

    public static bool HasItem(Item item) => Instance.inventoryManager.HasItem(item);

    private static void OnInventoryUpdate(ItemGroup group)
    {
        if (group.Item.Id != QuickSlotItem?.Id || group.Quantity > 0) return;

        SetQuickSlotItem(null);
    }

    [Command(Name = "give", Description = "Adds an item to the inventory.")]
    private void AddItemCommand(string id, int quantity = 1)
    {
        var item = ItemRegistry.Get(id);

        if (item is null)
        {
            DeveloperConsole.Console.Error.WriteLine($"Item '{id}' not found.");
            return;
        }

        inventoryManager.AddItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    [Command(Name = "take", Description = "Removes an item from the inventory.")]
    private void RemoveItemCommand(string id, int quantity = 1)
    {
        var item = ItemRegistry.Get(id);

        if (item is null)
        {
            DeveloperConsole.Console.Error.WriteLine($"Item '{id}' not found.");
            return;
        }

        inventoryManager.RemoveItem(new ItemGroup
        {
            Item = item,
            Quantity = quantity
        });
    }

    [Command(Name = "clear-inventory", Description = "Clears the inventory.")]
    private void ClearInventoryCommand()
    {
        inventoryManager.Clear();
        DeveloperConsole.Console.Out.WriteLine("Inventory cleared.");
    }
}