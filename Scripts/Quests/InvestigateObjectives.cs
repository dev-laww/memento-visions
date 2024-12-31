using System.Collections.Generic;
using Godot;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class InvestigateObjectives : QuestObjectives
{
    [ExportGroup("Investigation Targets")]
    [Export] public string[] TargetUniqueNames { get; set; } = System.Array.Empty<string>();
    [Export] public string[] TargetDisplayNames { get; set; } = System.Array.Empty<string>();
    private HashSet<string> investigatedTargets = new();
    public override void _Ready()
    {
        base._Ready();
        TargetCount = TargetUniqueNames.Length;
        currentCount = 0;
    }
    public void OnInteracted(string targetUniqueName)
    {
        if (System.Array.IndexOf(TargetUniqueNames, targetUniqueName) < 0) return;
        if (investigatedTargets.Contains(targetUniqueName)) return;

        investigatedTargets.Add(targetUniqueName);
        currentCount = investigatedTargets.Count;
        
        GD.Print($"Investigated {GetDisplayName(targetUniqueName)}");
        GD.Print($"Progress: {currentCount}/{TargetCount}");
        
        UpdateProgress();
        
        if (currentCount >= TargetCount)
        {
            ObjectiveComplete();
            GD.Print("All investigations complete!");
        }
    }
    private string GetDisplayName(string targetUniqueName)
    {
        var index = System.Array.IndexOf(TargetUniqueNames, targetUniqueName);
        return TargetDisplayNames[index];
    }
    public void StartInvestigation()
    {
        base.StartQuest();
        investigatedTargets.Clear();
        currentCount = 0;
        UpdateProgress();
    }
}