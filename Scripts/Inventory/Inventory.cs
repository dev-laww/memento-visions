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
		public Array<InventoryItems> Items { get; set; } = new Array<InventoryItems>();  
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
                Items = playerInventory.Items;
                GD.Print($"Loaded Inventory Count: {Items.Count}");
            }
            else
            {
                GD.Print("Failed to load player inventory.");
            }
        }

	public void AddItem(InventoryItems item)
		{
			
			GD.Print(Items.Count); 
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == null)
                {
                    Items[i] = item;
                    GD.Print($"Item added to slot {i}");
                    EmitSignal(SignalName.Update);
                    return;
                }
            }
			

    	}
	}

	
