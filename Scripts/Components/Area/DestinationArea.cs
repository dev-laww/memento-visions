using Godot;
using System;
using Game.Entities;
using Game.Quests;

namespace Game.Components.Area;
[Tool]
[GlobalClass]
public partial class DestinationArea : Area2D
{
    [Export] public Quest Quest { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        GD.Print($"{body.Name} entered the area.");
        
        if (Quest?.Objectives is EscortObjectives escortObjectives)
        {
            if (body is Node2D node2D)
            {
                escortObjectives.OnAreaEntered(node2D);
            }
            else
            {
                GD.PrintErr($"Body {body.Name} is not a Node2D!");
            }
        }
        else
        {
            GD.PrintErr("Quest objectives not set or not of type EscortObjectives!");
        }
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();

        if (Quest == null)
            warnings.Add("Quest is not set.");

        if (Quest != null && string.IsNullOrEmpty(Quest.QuestTitle))
            warnings.Add("QuestTitle is not set.");

        if (Quest?.Objectives is not EscortObjectives)
            warnings.Add("Quest objectives must be of type EscortObjectives!");

        return warnings.ToArray();
    }
}