using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Data;
using Godot;

namespace Game.Autoload;

[Icon("res://assets/icons/quest-manager.svg")]
public partial class QuestManager : Autoload<QuestManager>
{
    [Signal] public delegate void AddedEventHandler(Quest quest);
    [Signal] public delegate void UpdatedEventHandler(Quest quest);
    [Signal] public delegate void CompletedEventHandler(Quest quest);
    [Signal] public delegate void RemovedEventHandler(Quest quest);

    // TODO: move signals to source generator
    public static event AddedEventHandler QuestAdded
    {
        add => Instance.Added += value;
        remove => Instance.Added -= value;
    }

    public static event UpdatedEventHandler QuestUpdated
    {
        add => Instance.Updated += value;
        remove => Instance.Updated -= value;
    }

    public static event CompletedEventHandler QuestCompleted
    {
        add => Instance.Completed += value;
        remove => Instance.Completed -= value;
    }

    private readonly List<Quest> quests = [];

    public static IReadOnlyList<Quest> Quests => Instance.quests;

    public override void _EnterTree()
    {
        // TODO: load quests from save file
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        var completedQuests = new List<Quest>();

        foreach (var quest in quests)
        {
            quest.Update();

            if (quest.Completed)
            {
                EmitSignalCompleted(quest);
                completedQuests.Add(quest);
                Log.Debug($"{quest} completed.");
            }
            else
            {
                EmitSignalUpdated(quest);
            }
        }

        foreach (var quest in completedQuests)
        {
            Remove(quest.Id);
        }
    }

    public static bool IsActive(Quest quest) => Instance.quests.Contains(quest);

    public static void Add(Quest quest)
    {
        if (IsActive(quest)) return;

        PlayerInventoryManager.Pickup += quest.OnItemPickup;
        PlayerInventoryManager.Remove += quest.OnItemRemoved;

        GameEvents.ConnectToSignal(
            GameEvents.SignalName.EntityDied,
            CallableUtils.FromMethod(quest.OnEnemyDied)
        );

        Instance.quests.Add(quest);
        Instance.EmitSignalAdded(quest);

        Log.Info($"{quest} added.");
    }

    public static void Remove(string id)
    {
        var quest = Instance.quests.FirstOrDefault(q => q.Id == id);
        if (quest is null) return;


        PlayerInventoryManager.Pickup -= quest.OnItemPickup;
        PlayerInventoryManager.Remove -= quest.OnItemRemoved;

        GameEvents.DisconnectFromSignal(
            GameEvents.SignalName.EntityDied,
            CallableUtils.FromMethod(quest.OnEnemyDied)
        );

        Instance.quests.Remove(quest);
        Instance.EmitSignalRemoved(quest);

        Log.Info($"{quest} removed.");
    }
}