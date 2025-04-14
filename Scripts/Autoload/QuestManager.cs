using System.Collections.Generic;
using System.CommandLine.IO;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Data;
using Game.Utils.Extensions;
using Godot;

namespace Game.Autoload;

[Icon("res://assets/icons/quest_manager.svg")]
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
        base._EnterTree();

        CommandInterpreter.Register(this);

        foreach (var questId in SaveManager.Data.GetQuests())
        {
            var quest = QuestRegistry.Get(questId);

            if (quest is null)
            {
                Log.Error($"Quest {questId} not found.");
                continue;
            }

            Add(quest);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        CommandInterpreter.Unregister(this);

        var questsIds = quests.Select(q => q.Id).ToList();
        SaveManager.SetQuests(questsIds);
    }

    public override void _Process(double delta)
    {
        var completedQuests = new List<Quest>();

        foreach (var quest in quests)
        {
            quest.Update();

            if (quest.Completed)
            {
                EmitSignalCompleted(quest);
                completedQuests.Add(quest);
                Log.Debug($"{quest} completed.");

                this.GetPlayer()?.StatsManager.IncreaseExperience(quest.Experience);
                foreach (var item in quest.Items)
                {
                    PlayerInventoryManager.AddItem(item);
                }
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

    public static bool IsActive(Quest quest) => Instance.quests.Select(q => q.Id).Contains(quest.Id);

    public static void Add(Quest quest)
    {
        if (IsActive(quest)) return;

        PlayerInventoryManager.Pickup += quest.OnItemPickup;
        PlayerInventoryManager.Remove += quest.OnItemRemoved;

        GameEvents.EntityDied += quest.OnEnemyDied;

        Instance.quests.Add(quest);
        Instance.EmitSignalAdded(quest);
        SaveManager.AddQuest(quest.Id);

        Log.Info($"{quest} added.");
    }

    public static void Remove(string id)
    {
        var quest = Instance.quests.FirstOrDefault(q => q.Id == id);
        if (quest is null) return;


        PlayerInventoryManager.Pickup -= quest.OnItemPickup;
        PlayerInventoryManager.Remove -= quest.OnItemRemoved;

        GameEvents.EntityDied -= quest.OnEnemyDied;

        Instance.quests.Remove(quest);
        Instance.EmitSignalRemoved(quest);
        SaveManager.RemoveQuest(quest.Id);

        Log.Info($"{quest} removed.");
    }

    [Command(Name = "quest_add", Description = "Adds a quest to the quest manager.")]
    private void AddQuestCommand(string id)
    {
        var quest = QuestRegistry.Get(id);

        if (quest is null)
        {
            DeveloperConsole.Console.Error.WriteLine("Quest not found.");
            return;
        }

        if (quests.Contains(quest))
        {
            DeveloperConsole.Console.Error.WriteLine("Quest already added.");
            return;
        }

        Add(quest);
    }

    [Command(Name = "quest_remove", Description = "Removes a quest from the quest manager.")]
    private void RemoveQuestCommand(string id)
    {
        var quest = quests.FirstOrDefault(q => q.Id == id);

        if (quest is null)
        {
            DeveloperConsole.Console.Error.WriteLine("Quest not found.");
            return;
        }

        Remove(quest.Id);
    }
}