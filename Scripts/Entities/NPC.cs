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
        [Node]
        private Interaction interaction;

        [Export]
        private Resource DialogResource;

        [Export]
        private string DialogueStart = "Start";
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
            DialogueManager.ShowDialogueBalloon(DialogResource, DialogueStart);
            isDialogueActive = true; 
        }

        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated) return;

            WireNodes();
        }
    }
}