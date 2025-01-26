using System;
using System.Collections.Generic;
using System.Linq;
using Game.Globals;
using Godot;

namespace Game.Resources;

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
    }

    ~Quest()
    {
        if (Engine.IsEditorHint()) return;

        InventoryManager.Pickup -= OnItemPickup;
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

    private void OnItemPickup(ItemGroup item)
    {
        if (Completed) return;

        if (Ordered)
        {
            var objective = objectives[currentStep];

            if (objective.Type != QuestObjective.ObjectiveType.Collect) return;

            objective.UpdateItemProgress(item);
        }
        else
        {
            var objectives = Objectives.Where(objective => objective.Type == QuestObjective.ObjectiveType.Collect);

            foreach (var objective in objectives)
            {
                if (objective.Type != QuestObjective.ObjectiveType.Collect) continue;

                objective.UpdateItemProgress(item);
            }
        }

        Update();
    }

    public override string ToString() => $"<Quest ({Id})>";
}