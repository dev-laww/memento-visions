using Godot;
using GodotUtilities;

namespace Game.Quests;

[Scene]
public partial class SlayObjectives : QuestObjectives
{
	[Export]
	private int amount ;
	[Export]
	private string EnemyName;

	// public void connectSignals()
	// {
	// 	foreach (var Enemy in GetTree().GetNodesInGroup("enemies"))
	// 	{
	// 		Enemy.Connect("Died", new Callable(this, nameof(OnEnemyDied)));
	// 	}
	// }
	public void OnEnemyDied()
	{
		amount--;
		if (amount == 0)
		{
			ObjectiveComplete();
		}
	}
}

