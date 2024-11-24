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

        public override void _Ready()
        {
            if (interaction != null)
            {
                interaction.ShowUI();
            }
            else
            {
                GD.PrintErr("Interaction node is missing!");
            }
            interaction.Interacted += OnInteracted;

            // Connect the dialogue finished signal to a lambda expression
            DialogueManager.DialogueEnded += (Resource dialogueResource) =>
            {
                GD.Print("Dialogue finished");
                this.GetPlayer().SetProcessInput(true);
            };
        }

        private void OnInteracted()
        {
            this.GetPlayer().SetProcessInput(false);
            DialogueManager.ShowDialogueBalloon(DialogResource, DialogueStart);
        }

        public override void _Notification(int what)
        {
            if (what != NotificationSceneInstantiated) return;

            WireNodes();
        }
    }
}