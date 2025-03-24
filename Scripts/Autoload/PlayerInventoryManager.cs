using System.Collections.Generic;
using System.CommandLine.IO;
using Game.Common;
using Game.Common.Utilities;
using Game.Components;
using Game.Data;
using GodotUtilities;

namespace Game.Autoload;

[Scene]
public partial class PlayerInventoryManager : Autoload<PlayerInventoryManager>
{
    [Node] private InventoryManager inventoryManager;

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

    public override void _EnterTree()
    {
        base._EnterTree();
        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        SaveManager.Data.SetItemsData(inventoryManager.GetItemsAsModel());
        SaveManager.Save();
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

    public static void AddItem(ItemGroup item) => Instance.inventoryManager.AddItem(item);
    public static void RemoveItem(ItemGroup item) => Instance.inventoryManager.RemoveItem(item);
    public static void UseItem(ItemGroup item) => Instance.inventoryManager.UseItem(item);

    public static IReadOnlyList<ItemGroup> GetItemsFromCategory(Item.Category category) =>
        Instance.inventoryManager.GetItemsFromCategory(category);

    public static bool HasItem(ItemGroup group) => Instance.inventoryManager.HasItem(group);

    public static bool HasItem(Item item) => Instance.inventoryManager.HasItem(item);

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