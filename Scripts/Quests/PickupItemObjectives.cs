using Game.Components.Managers;
using Godot;
using GodotUtilities;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class PickupItemObjectives : QuestObjectives
{
	[Export] public string ItemId { get; set; }
	[Export] public bool ConsumeItems { get; set; } = true;

	public override void _Ready()
	{
		// Connect to inventory events
		
	}

	private void OnItemAdded(string itemId, int amount)
	{
		if (itemId != ItemId) return;

		currentCount += amount;
		UpdateProgress();

		if (ConsumeItems && currentCount >= TargetCount)
		{
			// Remove quest items from inventory

		}
	}

	public override void _ExitTree()
	{
	}
}

