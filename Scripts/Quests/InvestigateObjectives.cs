using Godot;
using GodotUtilities;

namespace Game.Quests
{
    [Tool]
    [GlobalClass]
    public partial class InvestigateObjectives : QuestObjectives
    {
        private void OnInteracted()
        {
            ObjectiveComplete();
            GD.Print("Investigation Complete");
        }

        private void StartInvestigation()
        {
            StartQuest();
            GD.Print("Investigation Started");
        }
    }
}