using Game.Inventory;
using Godot;
using System;

public partial class Collectables : Area2D
{
	[Export]
	public InventoryItems ItemRes { get; set; }

	
		public void collect (Inventory inventory)
	{
		 //called
		 InventorySlot newSlot = new InventorySlot
        {
            Item = ItemRes,  // Assign the InventoryItems to the InventorySlot
            Amount = 1       // Set the amount, this can be adjusted
        };

        inventory.AddItem(newSlot);
		this.QueueFree();
		
	}
}
