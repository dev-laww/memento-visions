using Godot;
using DialogueManagerRuntime;
using Game.Components.Area;
using Game.Utils.Extensions;
using GodotUtilities;

namespace Game.Entities
{
    [Scene]
    public partial class NPC : Node2D
    {
        [Node] private Interaction interaction;

        [Export] private Resource DialogResource;
        // [Export] private Quest Quest;

        private bool isDialogueActive;

        public override void _Ready()
        {
            interaction.Interacted += OnInteracted;

            DialogueManager.DialogueEnded += (Resource dialogueResource) =>
            {
                this.GetPlayer()?.SetProcessInput(true);
                isDialogueActive = false;
            };
        }

        private void OnInteracted()
        {
            if (isDialogueActive) return;

            this.GetPlayer()?.SetProcessInput(false);
            DialogueManager.ShowDialogueBalloon(DialogResource, "Start");
            isDialogueActive = true;
        }

        public void GiveQuest()
        {
            // Quest.StartQuest();
            // if (Quest.Objectives is DefenseObjectives defenseObjectives)
            // {
            //     defenseObjectives.StartDefense();
            // }
            //
        }

        public void CompleteQuest()
        {
            // if (Quest.Objectives is InvestigateObjectives investigateObjectives)
            // {
            //     investigateObjectives.OnInteracted("NPC");
            // }
            // Quest.DeliverQuest();
        }


        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated) return;

            WireNodes();
        }
    }
}