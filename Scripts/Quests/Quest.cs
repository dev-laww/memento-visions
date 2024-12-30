using Godot;
using System;
using System.Collections.Generic;
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
        Delivered
    }

    [Export] public string QuestTitle;
    [Export] public string QuestDescription;
    [Export] public QuestObjectives Objectives;
    [Export] public QuestStatus Status = QuestStatus.Available;
    [Export] public int Reward;
    [Export] public string[] QuestItems;
    [Export] public int Experience;
    
    public override void _Ready()
    {
    }
    

    public void StartQuest()
    {
        if (Status != QuestStatus.Available) return;
        Status = QuestStatus.Active;
        // Objective?.Initialize(this);
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
        // Objective?.Cleanup();
        QuestManager.RemoveQuest(this);
        GD.Print("Quest Delivered");
    }
}