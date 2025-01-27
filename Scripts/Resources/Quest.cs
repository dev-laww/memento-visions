using System;
using System.Collections.Generic;
using System.Linq;
using Game.Globals;
using Game.Registry;
using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest.svg")]
public partial class Quest : Resource
{
    [Export] public string Id { get; private set; } = Guid.NewGuid().ToString();
    [Export] public string Title;
    [Export] private bool Ordered;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] private QuestObjective[] objectives = [];

    [ExportCategory("Rewards")] [Export] private int Experience;
    [Export] private ItemGroup[] Items = [];

    public bool Completed { get; private set; }
    public List<QuestObjective> Objectives => [.. objectives];
    private int currentStep;

    public Quest()
    {
        if (Engine.IsEditorHint()) return;

        InventoryManager.Pickup += OnItemPickup;
        EnemyManager.EnemyDied += OnEnemyDied;
    }

    ~Quest()
    {
        if (Engine.IsEditorHint()) return;

        InventoryManager.Pickup -= OnItemPickup;
        EnemyManager.EnemyDied -= OnEnemyDied;
    }

    public void Update()
    {
        var completed = true;

        if (Ordered)
        {
            if (currentStep < objectives.Length && objectives[currentStep].Completed)
                currentStep++;

            completed = currentStep == objectives.Length;
        }
        else
        {
            if (objectives.Any(objective => !objective.Completed))
                completed = false;
        }

        if (!completed) return;

        Complete();
    }

    public void Complete()
    {
        if (Completed) return;

        Completed = true;
        GiveRewards();
        GD.Print($"{this} completed.");

        // TODO: save progress
    }

    private void GiveRewards()
    {
        if (!Completed) return;

        // TODO: give rewards
    }

    private void OnItemPickup(ItemGroup item) => ProcessObjectives(
        QuestObjective.ObjectiveType.Collect,
        objective => objective.UpdateItemProgress(item)
    );

    private void OnEnemyDied(Game.Entities.Enemies.Enemy enemy) => ProcessObjectives(
        QuestObjective.ObjectiveType.Kill,
        objective => objective.UpdateKillProgress(EnemyRegistry.Get(enemy.Id))
    );

    private void ProcessObjectives(
        QuestObjective.ObjectiveType type,
        Action<QuestObjective> processAction
    )
    {
        if (Completed) return;

        var targets = GetObjectives(type).ToList();

        targets.ForEach(processAction);

        Update();
    }

    private IEnumerable<QuestObjective> GetObjectives(QuestObjective.ObjectiveType type)
    {
        if (Ordered)
        {
            if (currentStep >= objectives.Length) yield break;
            var currentObjective = objectives[currentStep];
            if (currentObjective.Type == type)
                yield return currentObjective;
        }
        else
            foreach (var objective in objectives)
                if (objective.Type == type)
                    yield return objective;
    }
}