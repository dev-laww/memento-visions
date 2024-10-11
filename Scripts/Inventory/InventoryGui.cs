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
	  private Array <Node>slots;

	public override void _Ready()
	{
		 Inventory = (Inventory)ResourceLoader.Load("res://Scripts/Inventory/InventoryItems/PlayerInventory.tres");
		 GridContainer = GetNode<GridContainer>("NinePatchRect/GridContainer");
		 
		 slots = GridContainer.GetChildren();
		 update();
	

	}

public void _on_CloseButton_pressed(bool isopen) => EmitSignal(nameof(ClosedEventHandler));
public void _on_OpenButton_pressed(bool isopen) => EmitSignal(nameof(OpenedEventHandler));
	
	public bool isopen = true;
	public void open()
	{
		
		this.Visible = true;
		isopen = true;
	
		
	}

public void update()
{
	for (int i = 0; i < Inventory.Items.Count; i++)
	{
		ItemSlot itemSlot = (ItemSlot)slots[i];
		itemSlot.updateItem(Inventory.Items[i]);
		GD.Print("Item updated");
		GD.Print(Inventory.Items[i].Name);
	}
}

	public void close()
	{
		this.Visible = false;
		isopen = false;

	}
}
