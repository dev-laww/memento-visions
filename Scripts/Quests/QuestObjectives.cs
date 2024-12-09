using Godot;
using GodotUtilities;


namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class QuestObjectives : Quest
{
    [Export] public Quest quest;
   
    public void StartQuest()
    {
        GD.Print("Quest Started");
        quest.Status = QuestStatus.Active;
        QuestManager.Quests.Add(new Quest { QuestName = quest.QuestName, QuestDescription = quest.QuestDescription, Reward = quest.Reward, Experience = quest.Experience, Status = quest.Status });
        GD.Print("Quest Status: " + quest.Status);
        GD.Print("Quest Name: " + QuestName);
    }
    public void ObjectiveComplete()
    {
        GD.Print("Objective Complete");
        quest.Status = QuestStatus.Completed;
        GD.Print("Quest Status: " + quest.Status);
    }
    public void DeliverQuest()
    {
        GD.Print("Quest Delivered");
        quest.Status = QuestStatus.Delivered;
        GD.Print("Quest Status: " + quest.Status);
        //give reward
    }
}

