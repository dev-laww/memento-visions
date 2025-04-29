using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest.svg")]
public partial class Quest : Resource
{
    [Export] public string Id { get; private set; } = Guid.NewGuid().ToString();
    [Export] public string Title;
    [Export] public bool Ordered;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] private QuestObjective[] objectives;

    [ExportCategory("Rewards")][Export] public int Experience;
    [Export] public ItemGroup[] Items = [];

    public bool Completed { get; private set; }
    public List<QuestObjective> Objectives => [.. objectives];
    public QuestObjective CurrentObjective => objectives[currentStep];
    private int currentStep;
    public Common.Models.Quest Model => new()
    {
        Id = Id,
        Completed = Completed
    };

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

        if (!Completed)
        {
            Complete();
            QuestManager.Instance.EmitSignal(nameof(QuestManager.Completed), this);
        }
    }

    public void CompleteObjective(int index)
    {
        if (Completed) return;

        if (index < 0 || index >= objectives.Length) return;

        if (Ordered && index != currentStep) return;

        objectives[index].Complete();
        Update();
    }

    public void Complete()
    {
        if (Completed) return;

        Completed = true;
    }

    public void OnItemPickup(ItemGroup item) => ProcessObjectives(
        QuestObjective.ObjectiveType.Collect,
        objective => objective.UpdateItemProgress(item)
    );

    public void OnEnemyDied(Entity.DeathInfo info)
    {
        if (info.Killer is not Player || info.Victim is not Enemy) return;

        ProcessObjectives(
            QuestObjective.ObjectiveType.Kill,
            objective => objective.UpdateKillProgress(info.Victim as Enemy)
        );
    }

    public void OnItemRemoved(ItemGroup item) => ProcessObjectives(
        QuestObjective.ObjectiveType.Use,
        objective => objective.UpdateItemProgress(item)
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

    public override string ToString() => $"<Quest ({Id})>";
}