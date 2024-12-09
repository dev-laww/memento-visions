using Godot;
using GodotUtilities;

namespace Game.Quests;

[Scene]
public partial class Investigate : QuestObjectives
{
	private void OnInteracted()
	{
		ObjectiveComplete();
	}
}

