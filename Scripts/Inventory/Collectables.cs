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
		inventory.AddItem(ItemRes);
		this.QueueFree();
		
	}
}
