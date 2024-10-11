using Godot;
using Godot.Collections;
using System;
using MonoCustomResourceRegistry;

namespace Game.Inventory;

	[Tool]
	[RegisteredType(nameof(Inventory),"",nameof(Resource))]	
	public partial class Inventory : Resource
	{
		[Export]
		public Array<InventoryItems> Items { get; set; } = new Array<InventoryItems>();  


		public Inventory(){
			Items = new Array<InventoryItems>();

		}
	}
