using Godot;
using System;
using System.Collections.Generic;
using Game.Resources;
using GodotUtilities;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class Quest : Node
{
    public enum QuestStatus
    {
        Available,
        Active,
        Completed,
        Failed,
        Delivered
    }

    [Export] public string QuestTitle;
    [Export] public string QuestDescription;
    [Export] public QuestObjectives Objectives;
    [Export] public QuestStatus Status = QuestStatus.Available;
    [ExportGroup("Rewards")]
    [Export] public int Gold;
    [Export] public int Experience;
    [Export] public Item[] QuestItems;
   
   
    public override void _Ready()
    {
    }
    

    public void StartQuest()
    {
        if (Status != QuestStatus.Available) return;
        Status = QuestStatus.Active; ;
        QuestManager.AddQuest(this);
        QuestManager.NotifyQuestStarted(this);
        GD.Print("Quest Started");
    }

    public void CompleteQuest()
    {
        if (Status != QuestStatus.Active) return;
        Status = QuestStatus.Completed;
        QuestManager.NotifyQuestCompleted(this);
        GD.Print("Quest Completed");
    }

    public void DeliverQuest()
    {
        if (Status != QuestStatus.Completed) return;
        Status = QuestStatus.Delivered;

        QuestManager.RemoveQuest(this);
        GD.Print("Quest Delivered");
    }
    public void FailQuest()
    {
        if (Status != QuestStatus.Active) return;
        Status = QuestStatus.Failed;

        QuestManager.RemoveQuest(this);
        GD.Print("Quest Failed");
    }
}