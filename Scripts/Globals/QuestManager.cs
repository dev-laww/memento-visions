using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Resources;
using Godot;

namespace Game.Globals;

public partial class QuestManager : Global<QuestManager>
{
    public delegate void AddedEventHandler(Quest quest);
    public delegate void UpdatedEventHandler(Quest quest);
    public delegate void CompletedEventHandler(Quest quest);
    public delegate void RemovedEventHandler(Quest quest);

    public static event AddedEventHandler Added;
    public static event UpdatedEventHandler Updated;
    public static event RemovedEventHandler Removed;
    public static event CompletedEventHandler Completed;

    private readonly List<Quest> quests = [];

    public static IReadOnlyList<Quest> ActiveQuests => Instance.quests;

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        var completedQuests = new List<Quest>();

        foreach (var quest in quests)
        {
            quest.Update();

            if (quest.Completed)
            {
                Completed?.Invoke(quest);
                completedQuests.Add(quest);
                Log.Debug($"{quest} completed.");
            }
            else
            {
                Updated?.Invoke(quest);
            }
        }

        foreach (var quest in completedQuests)
        {
            Remove(quest.Id);
        }
    }

    public static void Add(Quest quest)
    {
        Instance.quests.Add(quest);
        Added?.Invoke(quest);

        EnemyManager.EnemyDied += quest.OnEnemyDied;

        Log.Info($"{quest} added.");
    }

    public static void Remove(string id)
    {
        var quest = Instance.quests.FirstOrDefault(q => q.Id == id);

        if (quest == null) return;

        Instance.quests.Remove(quest);
        Removed?.Invoke(quest);

        EnemyManager.EnemyDied -= quest.OnEnemyDied;

        Log.Info($"{quest} removed.");
    }
}