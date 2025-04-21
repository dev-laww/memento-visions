using System.Linq;
using DialogueManagerRuntime;
using Game.Autoload;
using Game.Common.Extensions;
using Game.Components;
using Game.Entities;
using Game.UI.Overlays;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;
using Game.Data;
using Quest = Game.Data.Quest;

namespace Game;

[Scene]
public partial class Lobby : Node2D
{
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private Interaction storyTellerInteraction;
    [Node] private Interaction blackSmithInteraction;
    [Node] private Interaction witchInteraction;

    [Node] private BlackSmith blackSmith;
    [Node] private Witch witch;
    [Node] private NPC lucas;

    private Quest quest2 = QuestRegistry.Get("sidequest:sidequest2");
    private Quest quest3 = QuestRegistry.Get("sidequest:sidequest3");
    private Quest quest4 = QuestRegistry.Get("sidequest:sidequest4");
    private int questFlag = 2;
    private bool isQuestActive = false;

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
        UpdateQuest();

        if (!SaveManager.Data.IntroShown)
        {
            var scene = resourcePreloader.GetResource<PackedScene>("Intro");
            var dialog = resourcePreloader.GetResource("intro");
            DialogueManager.ShowDialogueBalloonScene(scene, dialog);
        }

        if (!SaveManager.Data.NpcsEncountered.Contains(blackSmith.Id)) blackSmith.QueueFree();
        if (!SaveManager.Data.NpcsEncountered.Contains(witch.Id)) witch.QueueFree();
        if (!SaveManager.Data.NpcsEncountered.Contains(lucas.Id)) lucas.QueueFree();

        GD.Print(questFlag);
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

private void UpdateQuest()
{
    var quests = new[] { quest2, quest3, quest4 };
    isQuestActive = false;

    foreach (var quest in quests)
    {
        GD.Print(quest.Id);

        if (QuestManager.ActiveQuests.Any(q => q.Id == quest.Id))
        {
            isQuestActive = true;
            return;
        }
        if (QuestManager.CompletedQuests.Any(q => q.Id == quest.Id))
        {
            questFlag++;
            isQuestActive = false;
        }
    }
}

    private void GiveQuest2()
    {
        QuestManager.Add(quest2);
        isQuestActive = true;
    }

    private void GiveQuest3()
    {
        QuestManager.Add(quest3);
        isQuestActive = true;
    }

    private void GiveQuest4()
    {
        QuestManager.Add(quest4);
        isQuestActive = true;
    }
}