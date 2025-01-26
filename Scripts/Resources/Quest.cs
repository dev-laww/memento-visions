using System;
using System.Collections.Generic;
using System.Linq;
using Game.Registry;
using Godot;
using Godot.Collections;

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

    [ExportCategory("Rewards")]
    [Export] private int Experience;
    [Export] private ItemGroup[] Items = [];

    public bool Completed { get; private set; }
    public List<QuestObjective> Objectives => [.. objectives];
    private int currentStep;

    public void Update()
    {
        if (Completed || Engine.IsEditorHint()) return;

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

        // TODO: save progress
    }

    private void GiveRewards()
    {
        if (!Completed) return;

        // TODO: give rewards
    }
}