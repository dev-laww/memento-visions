using Godot;
using System;
using MonoCustomResourceRegistry;

namespace Game.Inventory;
    [Tool]
    [RegisteredType(nameof(InventorySlot),"",nameof(Resource))]
    public partial class InventorySlot : Resource
    {
        [Export]
        public InventoryItems Item { get; set; }
        [Export]
        public int Amount   { get; set; } 

        public InventorySlot()
        {
            Item = new InventoryItems();

            Amount = 50;
        }
        public void Assignments(InventoryItems item, int amount)
        {
            this.Item = item;
            this.Amount = amount;
            GD.Print($"Item: {item.Name} Amount: {amount}");
        }

    }
