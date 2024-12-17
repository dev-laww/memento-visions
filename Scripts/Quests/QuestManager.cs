using Godot;
using System;
using System.Collections.Generic;

namespace Game.Quests
{
    [Tool]
    [GlobalClass]
    public partial class QuestManager : Node
    {
        // Event to notify when quests change
        public static event Action OnQuestsChanged;

        public static List<Quest> Quests = new List<Quest>();

        public override void _Ready()
        {
            // Initialize quests or load from a saved state
        }

        public static void AddQuest(Quest quest)
        {
            if (!Quests.Contains(quest))
            {
                Quests.Add(quest);
                // Trigger the event to notify GUI and other listeners
                OnQuestsChanged?.Invoke();
            }
        }

        public static void RemoveQuest(Quest quest)
        {
            if (Quests.Contains(quest))
            {
                Quests.Remove(quest);
                // Trigger the event to notify GUI and other listeners
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

        public static void PrintAllQuests()
        {
            foreach (var quest in Quests)
            {
                GD.Print($"Quest: {quest.QuestTitle}, Status: {quest.Status}");
            }
        }
    }
}