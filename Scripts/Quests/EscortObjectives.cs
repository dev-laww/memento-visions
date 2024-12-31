using System.Linq;
using Game.Components.Managers;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class EscortObjectives : QuestObjectives
{

    [Export] public string EscortTargetName { get; set; }
    [Export] public string DestinationArea { get; set; }
    [Export] public float RequiredHealth { get; set; } = 50f;

    private Node2D escortTarget;
    private StatsManager targetStats;
    private bool isComplete;

    public override void _Ready()
    {
        base._Ready();
        FindEscortTarget();
        ConnectSignals();
        StartEscort();
    }

    private void FindEscortTarget()
    {
        escortTarget = GetTree().GetNodesInGroup("NPC")
            .OfType<Node2D>()
            .FirstOrDefault(target => target.Name == EscortTargetName);

    if (escortTarget == null) return;
    
        targetStats = escortTarget.GetNode<StatsManager>("StatsManager");
        if (targetStats == null)
        {
            GD.PrintErr($"StatsManager not found on escort target '{EscortTargetName}'!");
        }
        
    }

    private void ConnectSignals()
    {
        if (targetStats != null)
        {
            targetStats.StatsChanged += OnTargetHealthChanged;
        }
        
        GetTree().GetNodesInGroup("Area")
            .Where(area => area is Area2D && area.Name == DestinationArea)
            .Cast<Area2D>()
            .ToList()
            .ForEach(area => area.BodyEntered += OnAreaEntered);
    }

    private void StartEscort()
    {
        isComplete = false;
        currentCount = 0;
        TargetCount = 1; 
    }

    private void OnTargetHealthChanged(float value, StatsType stat)
    {
        if (stat != StatsType.Health || isComplete) return;

        if (value < RequiredHealth)
        {
            quest.FailQuest();
        }
    }

    public void OnAreaEntered(Node2D body)
    {
        if (isComplete || body != escortTarget) return;

        isComplete = true;
        currentCount = TargetCount;
        UpdateProgress();
        ObjectiveComplete();
    }

    public override float GetProgress()
    {
        return isComplete ? 1.0f : 0.0f;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        
        if (targetStats != null)
        {
            targetStats.StatsChanged -= OnTargetHealthChanged;
        }

        GetTree().GetNodesInGroup("Area")
            .Where(area => area is Area2D && area.Name == DestinationArea)
            .Cast<Area2D>()
            .ToList()
            .ForEach(area => area.BodyEntered -= OnAreaEntered);
    }
}
