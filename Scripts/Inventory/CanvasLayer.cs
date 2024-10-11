using Godot;
using GodotUtilities;

namespace Game.Inventory
{
	[Scene]
	public partial class CanvasLayer : Godot.CanvasLayer
	{
		[Node]
		public InventoryGui inventoryGui;

		public override void _Ready() 
		{
			 
			WireNodes();
			
			inventoryGui.close();

		}
	
		public void _on_inventory_gui_opened()
		{
			GD.Print("Inventory Signal opened");
			GetTree().Paused = true;
		}

		public void _on_inventory_gui_closed()
		{
			GD.Print("Inventory Signal closed");
			GetTree().Paused = false;
			
		}
		public override void _Input(InputEvent @event) 
		{
			if (@event.IsActionPressed("open_inventory")) 
			{
				if (inventoryGui.isopen) 
				{
				
					inventoryGui.close();
					GD.Print("Inventory closed");
				} 
				else 
				{
					
					inventoryGui.open();
					GD.Print("Inventory opened");
				}
			}
		}
	}
}
