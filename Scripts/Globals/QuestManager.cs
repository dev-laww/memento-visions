using System.Collections.Generic;
using Game.Resources;

namespace Game.Globals;

public partial class QuestManager : Global<QuestManager>
{
    public class ActiveQuest
    {
        public Quest Quest;
        public int Step;
        public int Progress;
        public bool Completed;
    }

    private readonly List<ActiveQuest> activeQuests = [];

    public static IReadOnlyList<ActiveQuest> ActiveQuests => Instance.activeQuests;

    public static void Add(Quest quest) => Instance.activeQuests.Add(new ActiveQuest { Quest = quest, Step = 0, Progress = 0, Completed = false });
}