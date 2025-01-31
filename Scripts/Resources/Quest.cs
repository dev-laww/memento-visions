using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Entities.Enemies;
using Game.Globals;
using Godot;
using Timer = System.Timers.Timer;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest.svg")]
public partial class Quest : Resource
{
    [Export] public string Id { get; private set; } = Guid.NewGuid().ToString();
    [Export] public string Title;
    [Export] private bool Ordered;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] private QuestObjective[] objectives;

    [ExportCategory("Rewards")] [Export] private int Experience;
    [Export] private ItemGroup[] Items = [];

    public bool Completed { get; private set; }
    public List<QuestObjective> Objectives => [.. objectives];
    private int currentStep;
    public bool IsActive => QuestManager.ActiveQuests.Contains(this);

    public Quest()
    {
        if (Engine.IsEditorHint()) return;

        var timer = new Timer(1000);
        timer.Elapsed += (_, _) =>
        {
            InventoryManager.Pickup += OnItemPickup;
            InventoryManager.Remove += OnItemRemoved;
            EnemyManager.EnemyDied += OnEnemyDied;
            timer.Dispose();
        };
        timer.AutoReset = false;
        timer.Start();
    }

    ~Quest()
    {
        if (Engine.IsEditorHint()) return;

        InventoryManager.Remove -= OnItemRemoved;
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

    public void CompleteObjective(int index)
    {
        if (Completed || !IsActive) return;

        if (index < 0 || index >= objectives.Length) return;

        if (Ordered && index != currentStep) return;

        objectives[index].Complete();
        Update();
    }

    public void Complete()
    {
        if (Completed) return;

        Completed = true;
        GiveRewards();
        Log.Info($"{this} completed.");

        // TODO: save progress
    }

    public void Start()
    {
        if (Completed || IsActive) return;

        QuestManager.Add(this);
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

    private void OnEnemyDied(Enemy enemy) => ProcessObjectives(
        QuestObjective.ObjectiveType.Kill,
        objective => objective.UpdateKillProgress(enemy)
    );

    private void OnItemRemoved(ItemGroup item) => ProcessObjectives(
        QuestObjective.ObjectiveType.Use,
        objective => objective.UpdateItemProgress(item)
    );

    private void ProcessObjectives(
        QuestObjective.ObjectiveType type,
        Action<QuestObjective> processAction
    )
    {
        if (Completed || !IsActive) return;

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