using Godot;
using System;
using System.Collections.Generic;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class QuestManager : Node
{
    // Event to notify when quests change
    public static event Action OnQuestsChanged;
    public static event Action<Quest> OnQuestStarted;
    public static event Action<Quest> OnQuestCompleted;

    public static List<Quest> Quests = new();

    public override void _Ready()
    {
        // Initialize quests or load from a saved state
    }

    public static void AddQuest(Quest quest)
    {
        if (!Quests.Contains(quest))
        {
            Quests.Add(quest);
            OnQuestsChanged?.Invoke();
        }
    }

    public static void RemoveQuest(Quest quest)
    {
        if (Quests.Contains(quest))
        {
            Quests.Remove(quest);
            OnQuestsChanged?.Invoke();
        }
    }

    public static Quest GetQuestByTitle(string title)
    {
        return Quests.Find(quest => quest.QuestTitle == title);
    }

    public static List<Quest> GetActiveQuests()
    {
        return Quests.FindAll(quest => quest.Status == Quest.QuestStatus.Active);
    }

    public static List<Quest> GetCompletedQuests()
    {
        return Quests.FindAll(quest => quest.Status == Quest.QuestStatus.Completed);
    }

    public static void NotifyQuestStarted(Quest quest)
    {
        OnQuestStarted?.Invoke(quest);
    }

    public static void NotifyQuestCompleted(Quest quest)
    {
        OnQuestCompleted?.Invoke(quest);
    }
}