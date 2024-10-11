using Game.Inventory;
using Godot;
using System;

public partial class ItemSlot : Panel
{
	private Sprite2D spriteItem;
	private Sprite2D background;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		spriteItem = GetNode<Sprite2D>("CenterContainer/Panel/Item");
		background = GetNode<Sprite2D>("Background");
		
	}
	public void updateItem(InventoryItems item)
	{
		if (item != null) 	
		{
			background.Frame=1;
			spriteItem.Visible = true;
			spriteItem.Texture = item.Texture;
			GD.Print("Item updated");
		}
		else
		{
			background.Frame=0;
			spriteItem.Visible = false;
			
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
