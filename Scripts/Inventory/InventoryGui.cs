using Godot;
using System;
using GodotUtilities;


namespace Game.Inventory;

	
public partial class InventoryGui : Control
{
	[Signal]
	public delegate void OpenedEventHandler();
	[Signal]
	public delegate void ClosedEventHandler();
	
	

	public bool isopen = true ;

	public void open()
	{
		
		this.Visible = true;
		isopen = true;
	
		
	}

	public void close()
	{
		this.Visible = false;
		isopen = false;

	}
}
