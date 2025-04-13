using Game.Autoload;
using Game.Components;
using Game.Entities;
using Game.UI.Overlays;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Lobby : Node2D
{
    [Node] private Interaction storyTellerInteraction;
    [Node] private Interaction blackSmithInteraction;
    [Node] private Interaction witchInteraction;

    [Node] private BlackSmith blackSmith;
    [Node] private Witch witch;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        storyTellerInteraction.Interacted += OnStoryTellerInteracted;
        blackSmithInteraction.Interacted += OnBlackSmithInteracted;
        witchInteraction.Interacted += OnWitchInteracted;
    }

    private void OnStoryTellerInteracted()
    {
        OverlayManager.ShowOverlay(OverlayManager.MODE_SELECT);
    }

    private void OnBlackSmithInteracted()
    {
        var craftingOverlay = (Crafting)OverlayManager.ShowOverlay(OverlayManager.CRAFTING);

        if (!IsInstanceValid(craftingOverlay)) return;

        craftingOverlay.ItemCrafted += OnCraft;
    }

    private void OnWitchInteracted()
    {
        var concoctOverlay = (Concoct)OverlayManager.ShowOverlay(OverlayManager.CONCOCT);

        if (!IsInstanceValid(concoctOverlay)) return;

        concoctOverlay.ItemCrafted += OnConcoct;
    }

    private void OnConcoct()
    {
        var player = this.GetPlayer();
        CinematicManager.StartCinematic(witch.GlobalPosition);
        player.InputManager.AddLock();
        witchInteraction.HideUI();

        witch.Work();

        GetTree().CreateTimer(4f).Timeout += () =>
        {
            witch.Idle();

            GetTree().CreateTimer(1f).Timeout += () =>
            {
                CinematicManager.EndCinematic();
                player.InputManager.RemoveLock();
                witchInteraction.ShowUI();
            };

            GetTree().CreateTimer(1.3f).Timeout += OnWitchInteracted;
        };
    }

    private void OnCraft()
    {
        var player = this.GetPlayer();
        CinematicManager.StartCinematic(blackSmith.GlobalPosition);
        player.InputManager.AddLock();
        blackSmithInteraction.HideUI();

        blackSmith.Work();

        GetTree().CreateTimer(4f).Timeout += () =>
        {
            blackSmith.Idle();

            GetTree().CreateTimer(1f).Timeout += () =>
            {
                CinematicManager.EndCinematic();
                player.InputManager.RemoveLock();
                blackSmithInteraction.ShowUI();
            };

            GetTree().CreateTimer(1.3f).Timeout += OnBlackSmithInteracted;
        };
    }
}