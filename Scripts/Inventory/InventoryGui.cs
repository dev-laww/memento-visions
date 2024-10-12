using Godot;
using System;
using GodotUtilities;
using System.Diagnostics.Tracing;
using Godot.Collections;


namespace Game.Inventory;


public partial class InventoryGui : Control
{
	[Signal]
	public delegate void OpenedEventHandler();
	[Signal]	
	public delegate void ClosedEventHandler();
	private Inventory Inventory { get; set; }
	  [Node]
	  private GridContainer GridContainer;
	  private Array <Node>Slots;
	

	public override void _Ready()
	{
		 Inventory = (Inventory)ResourceLoader.Load("res://Scripts/Inventory/InventoryItems/PlayerInventory.tres");
		 GridContainer = GetNode<GridContainer>("NinePatchRect/GridContainer");
		 Inventory.Update += InventoryUpdateSignal;
		 Slots = GridContainer.GetChildren();
		 update();
	

	}

private void InventoryUpdateSignal()
{
	GD.Print("Inventory updated"); //hindi to nagana  :>
	update();
}

	public bool isopen = true;
	public void open()
	{	
		this.Visible = true;
		isopen = true;
		update();
		EmitSignal(SignalName.Opened);
		
	}



	public void close()
	{
		this.Visible = false;
		isopen = false;
		EmitSignal(SignalName.Closed);

	}

	public void update()
	{
		GD.Print("Inventorygui update called");
		for (int i = 0; i < Inventory.Items.Count; i++)
		{
			Slots[i].Call("updateItem", Inventory.Items[i]);
		}
	}	
}
