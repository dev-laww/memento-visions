using Godot;
using DialogueManagerRuntime;
using Game.Utils.Extensions;
using GodotUtilities;
namespace Game.Components.Area;

[Tool]
[Scene]
public partial class CutsceneTrigger : Area2D
{
	
	[Export]
	private Resource DialogResource;

	[Export]
	private string DialogueStart = "Start";
	private bool IsDialogueActive = true;
	public override void _Ready()
	{
		
		BodyEntered += OnBodyEntered;
		
		DialogueManager.DialogueEnded += (Resource dialogueResource) =>
		{
			GD.Print("Dialogue finished");
			this.GetPlayer().SetProcessInput(true);
		};
	}
	public void OnBodyEntered(Node body)
	{
		if (IsDialogueActive)
		{
			GD.Print("Area entered");
			var player = this.GetPlayer();
			player.velocity.Decelerate();
			player.SetProcessInput(false);
			DialogueManager.ShowDialogueBalloon(DialogResource, DialogueStart);
			IsDialogueActive = false;
		}
	}
	public override void _Notification(int what)
	{
		if (what != NotificationSceneInstantiated) return;

		WireNodes();
	}

}
