using Godot;
using Godot.Collections;
using System;
using MonoCustomResourceRegistry;
using System.Security.Cryptography.X509Certificates;


namespace Game.Inventory;

	[Tool]
	[RegisteredType(nameof(Inventory),"",nameof(Resource))]	

	public partial class Inventory : Resource
	{
		[Export]
		public Array<InventorySlot> Slots { get; set; } = new Array<InventorySlot>();  
		[Signal]
		public delegate void UpdateEventHandler();

		
		public Inventory()
		{
			
			 LoadPlayerInventory();

		}
		public void EmitUpdateSignal()
		{
			EmitSignal(SignalName.Update);
		}
		private void LoadPlayerInventory()
        {
            var playerInventory = (Inventory)ResourceLoader.Load("res://Scripts/Inventory/InventoryItems/PlayerInventory.tres");

            if (playerInventory != null)
            {
                Slots = playerInventory.Slots;
                GD.Print($"Loaded Inventory Count: {Slots.Count}");
            }
            else
            {
                GD.Print("Failed to load player inventory.");
            }
        }

	public void AddItem(InventorySlot slot ) 
		{
			 GD.Print(Slots.Count); 
    for (int i = 0; i < Slots.Count; i++)
    {   
        for (slot.Amount = 1; slot.Amount <= 50; slot.Amount++)
        {
            if (Slots[i] != null && Slots[i].Item == slot.Item)
            {
                Slots[i].Amount += slot.Amount;
                GD.Print($"Item {slot.Item.Name} added to slot {i}");
                EmitSignal(SignalName.Update);
                return;
            }
        }
    
        if (Slots[i] == null) 
        {
            // Create a new InventorySlot and assign the item to it
            InventorySlot newSlot = new InventorySlot
            {
                Item = slot.Item, 
                Amount = 1
            };

            Slots[i] = newSlot;
            GD.Print($"Item {slot.Item.Name} added to slot {i}");
            EmitSignal(SignalName.Update);
            return;
        }
    }

    GD.Print("No empty slots available.");
}
	}

	
