using Game.Inventory;
using Godot;
using System;

public partial class ItemSlot : Panel
{
	private Sprite2D spriteItem;
	private Sprite2D background;
	private Label quantityLabel;

	public override void _Ready()
	{
		spriteItem = GetNode<Sprite2D>("CenterContainer/Panel/Item");
		background = GetNode<Sprite2D>("Background");
		quantityLabel = GetNode<Label>("CenterContainer/Panel/Quantity");
		
	}
	public void updateItem(InventorySlot slot)
	{
		if (slot != null) 	
		{
			background.Frame=1;
			spriteItem.Visible = true;
			spriteItem.Texture = slot.Item.Texture;
			quantityLabel.Visible = true;
			quantityLabel.Text = slot.Amount.ToString();
		
		}
		else
		{
			background.Frame=0;
			spriteItem.Visible = false;
			quantityLabel.Visible = false;
			
		}
	}

	
}
