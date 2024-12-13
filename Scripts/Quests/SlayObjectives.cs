using Godot;
using GodotUtilities;

namespace Game.Quests
{
    [Tool]
    [GlobalClass]
    public partial class SlayObjectives : QuestObjectives
    {
        [Export]
        private int amount;
        [Export]
        private string EnemyName;

        // connect signal on enemy dies call OnEnemyDied method
        public void OnEnemyDied()
        {
            amount--;
            if (amount == 0)
            {
                ObjectiveComplete();
            }
        }
    }
}