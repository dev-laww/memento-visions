using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Entities.Player;
using Game.Resources;
using Game.UI;
using Game.Utils.Extensions;
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
   [Signal] public delegate void OnQuestCompletedEventHandler();
   
   
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
        EmitSignal(SignalName.OnQuestCompleted);
    }

    public void DeliverQuest()
    {
        var playerInventory = this.GetPlayer()?.Inventory;
        if (playerInventory == null)
        {
            GD.PrintErr("Player inventory not found!");
            return;
        }
        if (Status != QuestStatus.Completed) return;
        Status = QuestStatus.Delivered;
        // Player.AddGold(Gold);
        // Player.AddExperience(Experience);
        foreach (var item in QuestItems)
        {
            playerInventory.AddItem(item);
        }
        

        QuestManager.RemoveQuest(this);
        GD.Print("Quest Delivered");
    }
    public async Task FailQuest()
    {
        if (Status != QuestStatus.Active) return;
        Status = QuestStatus.Failed;

        QuestManager.RemoveQuest(this);
        GD.Print("Quest Failed");
        await ToSignal(GetTree().CreateTimer(5), "timeout");
        Status = QuestStatus.Available;
    }
}