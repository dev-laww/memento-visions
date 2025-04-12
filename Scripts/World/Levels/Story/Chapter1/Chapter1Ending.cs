using System;
using Godot;
using Game.Autoload;
using Game.Components;
using Game.Entities;
using GodotUtilities;
using Game.UI.Overlays;
using Game.Utils.Extensions;

namespace Game.World;

using DialogueManagerRuntime;

[Scene]
public partial class Chapter1Ending : BaseLevel
{
    [Node] private Interaction storyTellerInteraction;
    [Node] private Interaction blackSmithInteraction;
    [Node] private Interaction witchInteraction;
    [Node] private TransitionArea transitionArea;

    [Node] private BlackSmith blackSmith;

    [Node] private Witch witch;

    [Node] private StoryTeller storyTeller;
    public bool isChiefInteracted = false;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        transitionArea.Toggle(false);
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

        craftingOverlay.ItemCrafted += OnCraft;
    }

    private void OnWitchInteracted()
    {
        var concoctOverlay = (Concoct)OverlayManager.ShowOverlay(OverlayManager.CONCOCT);

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

    public void StartCutscene(Vector2 targetPosition)
    {
        CinematicManager.StartCinematic();
        MoveCameraTo(targetPosition, 2.5f,
            () => {  CinematicManager.EndCinematic(); });
    }

    public static void MoveCameraTo(Vector2 position, float duration, Action onComplete = null)
    {
        GameCamera.SetTargetPositionOverride(position);
        var timer = GameCamera.Instance.GetTree().CreateTimer(duration);
        timer.Timeout += () => { onComplete?.Invoke(); };
    }

    public void StoryTellerCutscene()
    {
        StartCutscene(storyTeller.GlobalPosition);
    }

    public void BlackSmithCutscene()
    {
        StartCutscene(blackSmith.GlobalPosition);
    }

    public void WitchCutscene()
    {
        StartCutscene(witch.GlobalPosition);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        storyTellerInteraction.Interacted -= OnStoryTellerInteracted;
        blackSmithInteraction.Interacted -= OnBlackSmithInteracted;
        witchInteraction.Interacted -= OnWitchInteracted;
    }
}