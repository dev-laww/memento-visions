using System.Collections.Generic;
using System.Collections.ObjectModel;
using Game.Resources;
using Godot;

namespace Game.Components.Managers;

public partial class InventoryManager : Node
{
    [Signal] public delegate void ItemAddedEventHandler(Item item);

    private readonly ReadOnlyDictionary<ItemCategory, List<Item>> Inventory = new(
        new Dictionary<ItemCategory, List<Item>>
        {
            { ItemCategory.Weapon, [] },
            { ItemCategory.Quest, [] },
            { ItemCategory.Consumable, [] },
            { ItemCategory.Material, [] }
        }
    );

    private void AddItem(Item item)
    {
        Inventory[item.Category].Add(item);
        EmitSignal(SignalName.ItemAdded, item);
    }

    private void RemoveItem(Item item) => Inventory[item.Category].Remove(item);
    public bool HasItem(Item item) => Inventory[item.Category].Contains(item);
}