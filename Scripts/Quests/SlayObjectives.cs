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

         public override void _Ready()
         {
             var enemies = GetTree().GetNodesInGroup("Enemies");
             foreach (var enemy in enemies)
             {
                 if (enemy is Samurai samurai)
                 {
                     samurai.EnemyDied += OnEnemyDied;
                 }
             }
         }

         public void OnEnemyDied (string EnemyKilled)
         {
             GD.Print("Enemy Killed: " + EnemyKilled + " Enemy Name: " + EnemyName);
             if (EnemyKilled != EnemyName) return;
             if (++amount == TargetCount)
             {
                 GD.Print(amount);
                 ObjectiveComplete();
             }
         }
     }
 }