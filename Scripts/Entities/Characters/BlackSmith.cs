using Game.Autoload;
using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class BlackSmith : Entity
{
    [Node] private Interaction interaction;
    [Node] private AnimationTree animationTree;
    [Node] private Timer hammerUpTimer;

    private AnimationNodeStateMachinePlayback playback;
    private string currentAnimation = "hammer_up";

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        interaction.Interacted += OnInteracted;
        hammerUpTimer.Timeout += OnHammerUpTimeout;

        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
    }

    private static void OnInteracted()
    {
        OverlayManager.ShowOverlay(OverlayManager.CRAFTING);
    }

    private void OnHammerUpTimeout()
    {
        playback.Travel(currentAnimation);
        hammerUpTimer.Call("start_random");
        currentAnimation = currentAnimation == "breath" ? "hammer_up" : "breath";
    }

    public void Work()
    {
        var player = this.GetPlayer();
        CinematicManager.StartCinematic(GlobalPosition);
        player.InputManager.AddLock();
        interaction.HideUI();

        playback.Travel("work");

        // GetTree().CreateTimer(0.2f).Timeout += player.InputManager.AddLock;
        GetTree().CreateTimer(4f).Timeout += () =>
        {
            playback.Travel("breath");

            GetTree().CreateTimer(1f).Timeout += () =>
            {
                CinematicManager.EndCinematic();
                player.InputManager.RemoveLock();
                interaction.ShowUI();
            };

            GetTree().CreateTimer(1.3f).Timeout += OnInteracted;
        };
    }
}