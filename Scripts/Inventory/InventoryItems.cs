using Godot;
using System;
using MonoCustomResourceRegistry;

namespace Game.Inventory;

	[Tool]
	[RegisteredType(nameof(InventoryItems),"",nameof(Resource))]
	public partial class InventoryItems : Resource
	{
		[Export]
		public string Name { get; set; } = ""; 

		[Export]
		public Texture2D Texture { get; set; } 


		public InventoryItems(){
			Name = "";
			Texture = new Texture2D();

	}
}
