using Game.Components.Managers;
using Game.Resources;
using Godot;
using Game.Utils.Extensions;
using GodotUtilities;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class PickupItemObjectives : QuestObjectives
{
	[Export] public string itemUniqueName { get; set; }
	private InventoryManager InventoryManager;

	public override void _Ready()
	{ 
		var playerInventory = this.GetPlayer()?.Inventory;
		if(playerInventory == null) return;
		playerInventory.ItemAdd += OnItemAdded;
	}
	

	private void OnItemAdded(Item item)
	{
		if (item.UniqueName != itemUniqueName) return;

		currentCount += item.Value;
		GD.Print(item.Value);
		UpdateProgress();
	}

	public override void _ExitTree()
	{
	}
}

