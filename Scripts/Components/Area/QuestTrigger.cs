using Godot;
using System;
using Game.Quests;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class QuestTrigger : Area2D
{
    [Export] private Quest Quest;

    [Signal] public delegate void DiedEventHandler();

    private bool triggered;
    private SlayObjectives SlayObjectives = new();


    public override void _Ready()
    {
        CollisionMask = 1 << 2;
        CollisionLayer = 1 << 4;
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (triggered) return;
        Quest.StartQuest();
        triggered = true;
    }


    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();

        if (Quest == null)
            warnings.Add("Quest is not set.");

        if (string.IsNullOrEmpty(Quest.QuestTitle))
            warnings.Add("TargetId is not set.");
        return warnings.ToArray();
    }
}