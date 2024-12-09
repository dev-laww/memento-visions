using Godot;
using System;
using GodotUtilities;

namespace Game.Quests;
[Tool]
[GlobalClass]


public partial class Quest : Node
{
    [Export] public string QuestName;
    [Export] public string QuestDescription;
    // [Export] public int QuestID; thinking about adding for quest pointer

    public enum QuestStatus
    {
        Available,
        Active,
        Completed,
        Delivered
    }

    [Export] public QuestStatus Status = QuestStatus.Active;
    [Export] public int Reward;
    [Export] public string[] QuestItems;
    [Export] public int Experience;
}
