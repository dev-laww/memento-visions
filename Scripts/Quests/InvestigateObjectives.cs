using Godot;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class InvestigateObjectives : QuestObjectives
{
    public void OnInteracted()
    {
        ObjectiveComplete();
        GD.Print("Investigation Complete");
    }

    public void StartInvestigation()
    {
        StartQuest();
    }
}