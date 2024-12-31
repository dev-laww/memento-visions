using System.Linq;
using Godot;
using Game.Components.Managers;
using GodotUtilities;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class DefenseObjectives : QuestObjectives
{
    [Export] public string DefenseTargetName { get; set; }
    [Export] public float DefenseDuration { get; set; } = 60f;
    [Export] public float RequiredHealth { get; set; } = 50f;
    private Node2D defenseTarget;
    private float elapsedTime;
    private bool isFailed;
    private StatsManager targetStats;

    public override void _Ready()
    {
        base._Ready();
        FindDefenseTarget();
        ConnectSignals();
    }

    private void FindDefenseTarget()
    {
        defenseTarget = GetTree().GetNodesInGroup("NPC")
            .OfType<Node2D>()
            .FirstOrDefault(target => target.Name == DefenseTargetName);

        if (defenseTarget == null)
        {
            GD.PrintErr($"Defense target '{DefenseTargetName}' not found!");
            return;
        }

        targetStats = defenseTarget.GetNode<StatsManager>("StatsManager");
        if (targetStats == null)
        {
            GD.PrintErr($"StatsManager not found on defense target '{DefenseTargetName}'!");
        }
    }

    private void ConnectSignals()
    {
        if (targetStats != null)
        {
            targetStats.StatsChanged += OnTargetHealthChanged;
        }
    }

    public void StartDefense()
    {
        quest.StartQuest();
        elapsedTime = 0;
        isFailed = false;
        currentCount = 0;
        TargetCount = 100; 
    }

    public override void _Process(double delta)
    {
        if (isFailed || quest.Status != Quest.QuestStatus.Active) return;

        elapsedTime += (float)delta;
        currentCount = (int)((elapsedTime / DefenseDuration) * 100);
        
        if (elapsedTime >= DefenseDuration)
        {
            ObjectiveComplete();
            return;
        }

        UpdateProgress();
    }

    private void OnTargetHealthChanged(float value, StatsType stat)
    {
        if (stat != StatsType.Health || isFailed) return;

        if (value < RequiredHealth)
        {
            isFailed = true;
            quest.FailQuest();
            
        }
    }

    public override float GetProgress()
    {
        return elapsedTime / DefenseDuration;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (targetStats != null)
        {
            targetStats.StatsChanged -= OnTargetHealthChanged;
        }
    }
}