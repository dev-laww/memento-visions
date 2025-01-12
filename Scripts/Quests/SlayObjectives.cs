using System.Linq;
using Godot;
using Game.Enemy.Common;
using Game.Entities;
using Game.Utils.Battle;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class SlayObjectives : QuestObjectives
{
    [Export] private string UniqueName;

    private int amount;

    public override void _Ready()
    {
        GetTree().GetNodesInGroup("Enemy")
            .Where(enemy => enemy is Entity)
            .Cast<Entity>()
            .ToList()
            .ForEach(enemy => enemy.Death += OnEnemyDied);
    }

    public void OnEnemyDied(Entity enemy)
    {
        if(quest.Status != Quest.QuestStatus.Active) return;
        var EnemyKilled = enemy.UniqueName;

        if (EnemyKilled != UniqueName) return;
        if (amount == TargetCount) return;

        currentCount++;
        UpdateProgress();
        GD.Print(currentCount);
    }
}