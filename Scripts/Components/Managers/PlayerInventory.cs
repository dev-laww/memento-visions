using Godot;
using System;
using System.Collections.Generic;
using Game.Resources;

namespace Game.Components.Managers;

public partial class PlayerInventory : Node
{
	public List<Item> Inventory;
	private int MaxInventorySize = 20;
	
	public override void _Ready()
	{
		Inventory = new List<Item>();
		
		
	}

	public void AddItem(Item item)
	{
		Inventory.Add(item);
	}
	
	public void RemoveItem(Item item)
	{
		Inventory.Remove(item);
	}
	public void PrintInventory()
	{
		var sword = GD.Load<Item> ("res://resources/inventory/temps/redsword.tres");
		var rock = GD.Load<Item> ("res://resources/inventory/temps/rock.tres");
		Inventory.Add(sword);
		Inventory.Add(rock);
		
		foreach (var item in Inventory)
		{
			GD.Print($"Item: {item.Name}");
		}
	}
}
