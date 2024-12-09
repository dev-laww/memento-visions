using Godot;
using System;
using System.Collections;

namespace Game.Quests;
    [Tool]
    [GlobalClass]

public partial class QuestManager : Node
{
   public static ArrayList Quests = new ArrayList();
    public override void _Ready()
    {
       Quests.Add(new Quest { QuestName = "Test Quest", QuestDescription = "This is a test quest", Reward = 100, Experience = 100, Status = Quest.QuestStatus.Active });

    }


}
