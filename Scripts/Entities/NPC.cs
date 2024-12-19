using Godot;
using DialogueManagerRuntime;
using Game.Components.Area;
using Game.Quests;
using Game.Utils.Extensions;
using GodotUtilities;

namespace Game.Entities
{
    [Scene]
    public partial class NPC : Node2D
    {
        [Node]
        private Interaction interaction;

        [Export]
        private Resource DialogResource;
        [Export]
        private Quest Quest;
        private bool isDialogueActive = false;

        public override void _Ready()
        {
            interaction.Interacted += OnInteracted;

            DialogueManager.DialogueEnded += (Resource dialogueResource) =>
            {
                GD.Print("Dialogue finished");
                this.GetPlayer().SetProcessInput(true);
                isDialogueActive = false;
            };
        }

        private void OnInteracted()
        {
            if (isDialogueActive) return;

            this.GetPlayer().SetProcessInput(false);
            DialogueManager.ShowDialogueBalloon(DialogResource, "Start");
            isDialogueActive = true;

            if (Quest == null)
            {
                GD.PrintErr("Quest is not assigned to this NPC.");
                return;
            }

            if (Quest.Status == Quest.QuestStatus.Available)
            {
                Quest.StartQuest();
                Quest.PrintQuest();
            }
        }


        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated) return;

            WireNodes();
        }
    }
}