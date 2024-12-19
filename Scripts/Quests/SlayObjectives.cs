using Game.Components.Area;
using Game.Components.Managers;
using Godot;
using GodotUtilities;
using Game.Enemy.Common;

namespace Game.Quests
{
    [Tool]
    [GlobalClass]
    public partial class SlayObjectives : QuestObjectives
    {
        [Export]
        private string EnemyName;
        private int amount = 0;
        private Samurai samurai;
       
        public override void _Ready()
        {  
        
        }
        public void OnEnemyDied (string EnemyKilled)
        {
            if (EnemyKilled != EnemyName) return;
            if (++amount == TargetCount)
            {
                ObjectiveComplete();
            }
        }
    }
}