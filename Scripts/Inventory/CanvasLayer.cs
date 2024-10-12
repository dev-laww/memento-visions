using Godot;
using GodotUtilities;

namespace Game.Inventory;

	[Scene]
	public partial class CanvasLayer : Godot.CanvasLayer
	{
		[Node]
		public InventoryGui inventoryGui;

		public override void _Notification(int what) 
		{
			if (what != NotificationSceneInstantiated) return;
			WireNodes();
		}
		public override void _Ready() 
		{
			 
			WireNodes();
			inventoryGui.close();
			inventoryGui.Opened += OnInventoryGuiOpened;
			inventoryGui.Closed += OnInventoryGuiClosed;

		}
	
		public void OnInventoryGuiOpened()
		{
			// GetParent().GetTree().Paused = true; nagpapause lahat
			GD.Print("Inventory Signal opened");
		
		}

		public void OnInventoryGuiClosed()
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
