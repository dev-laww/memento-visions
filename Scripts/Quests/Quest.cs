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

    public void StartQuest()
    {
        Status = QuestStatus.Active;
        GD.Print("Quest Started");
        QuestManager.AddQuest(this);
    }

    public void CompleteQuest()
    {
        Status = QuestStatus.Completed;
        GD.Print("Quest Completed");
    }

    public void PrintQuest()
    {
        foreach (var quest in QuestManager.Quests)
        {
            GD.Print(quest.QuestTitle);
            GD.Print("Quest Status: " + quest.Status);
        }
    }
}