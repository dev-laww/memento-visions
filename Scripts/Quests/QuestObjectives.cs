using Godot;
using GodotUtilities;


namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class QuestObjectives : Node
{
    [Export] public Quest quest { get; set; }

    [Export] public int TargetCount;

    public void StartQuest()
    {
        quest.Status = Quest.QuestStatus.Active;
    }

    public void ObjectiveComplete()
    {
        quest.Status = Quest.QuestStatus.Completed;
    }

    public void DeliverQuest()
    {
        quest.Status = Quest.QuestStatus.Delivered;

        //give reward
    }
}