using Game.Components.Area;
using Game.Components.Managers;
using Godot;
using GodotUtilities;

namespace Game.Quests
{
    [Tool]
    [GlobalClass]
    public partial class SlayObjectives : QuestObjectives
    {
        [Export]
        private string EnemyName;
        private int amount = 0;
       
        public override void _Ready()
        {  
         GD.Print(TargetCount);
        }
        public void OnEnemyDied (string EnemyKilled)
        {
            if (EnemyKilled != EnemyName) return;
            if (++amount == TargetCount)
            {
                ObjectiveComplete();
                GD.Print("Slay Objective Complete");
            }
        }
    }
}