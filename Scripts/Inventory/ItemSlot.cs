using Game.Inventory;
using Godot;
using System;

public partial class ItemSlot : Panel
{
	private Sprite2D spriteItem;
	private Sprite2D background;

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
		
		}
		else
		{
			background.Frame=0;
			spriteItem.Visible = false;
			
		}
	}

	
}
