using System.Collections.Generic;
using Game.Resources;
using Godot;

namespace Game.Globals;

public partial class QuestManager : Global<QuestManager>
{
    public partial class ActiveQuest : GodotObject
    {
        public Quest Quest;
        public int Step;
        public int Progress;
        public bool Completed;
    }

    [Signal] public delegate void QuestAddedEventHandler(ActiveQuest activeQuest);
    [Signal] public delegate void QuestUpdatedEventHandler(ActiveQuest activeQuest);
    [Signal] public delegate void QuestCompletedEventHandler(ActiveQuest activeQuest);

    private readonly List<ActiveQuest> activeQuests = [];

    public static IReadOnlyList<ActiveQuest> ActiveQuests => Instance.activeQuests;

    public static void Add(Quest quest)
    {
        Instance.activeQuests.Add(new ActiveQuest { Quest = quest, Step = 0, Progress = 0, Completed = false });
        Instance.EmitSignal(SignalName.QuestAdded, Instance.activeQuests[^1]);
    }
    
    // TODO: Implement the rest of the QuestManager class
}