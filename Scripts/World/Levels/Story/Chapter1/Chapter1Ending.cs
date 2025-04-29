using System;
using System.Linq;
using Godot;
using Game.Autoload;
using Game.Components;
using Game.Entities;
using GodotUtilities;
using Game.UI.Overlays;
using Game.Utils.Extensions;
using Game.Data;
using Quest = Game.Data.Quest;

namespace Game.World;

using DialogueManagerRuntime;

[Scene]
public partial class Chapter1Ending : BaseLevel
{
    [Node] private Interaction storyTellerInteraction;
    [Node] private Interaction blackSmithInteraction;
    [Node] private Interaction witchInteraction;
    [Node] private NPC lucas;
    [Node] private TransitionArea transitionArea;
    [Node] private ScreenMarker screenMarker;

    [Node] private BlackSmith blackSmith;

    [Node] private Witch witch;

    [Node] private StoryTeller storyTeller;
    public bool isChiefInteracted = false;
    private Quest quest2 = QuestRegistry.Get("sidequest:sidequest2");
    private Quest quest3 = QuestRegistry.Get("sidequest:sidequest3");
    private Quest quest4 = QuestRegistry.Get("sidequest:sidequest4");
    private Quest mainQuest = QuestRegistry.Get("mainquest:mainquest"); //lagay mo dito id nung mainquest
    private int questFlag = 2;
    private bool isQuestActive = false;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        transitionArea.Toggle(false);
        screenMarker.Toggle(true);
        storyTellerInteraction.Interacted += OnStoryTellerInteracted;
        blackSmithInteraction.Interacted += OnBlackSmithInteracted;
        witchInteraction.Interacted += OnWitchInteracted;
        QuestManager.QuestCompleted += UpdateQuest;
        UnlockRecipes();
        UnlockNPC();
        UpdateQuest(quest2);
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

    private void OnChiefInteracted()
    {
        screenMarker.Toggle(false);
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
        MoveCameraTo(targetPosition, 2.5f, CinematicManager.EndCinematic);
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

    public void LucasCutscene()
    {
        StartCutscene(lucas.GlobalPosition);
    }

    public static void UnlockRecipes()
    {
        SaveManager.UnlockRecipe("id:taho");
        SaveManager.UnlockRecipe("item:salabat");
        SaveManager.UnlockRecipe("item:puto");
        SaveManager.UnlockRecipe("item:bibingka");
        SaveManager.UnlockRecipe("item:breeze_potion");
        SaveManager.UnlockRecipe("weapon:sword");
    }

    //temporary function for demo
    public static void UnlockNPC()
    {
        SaveManager.AddNpcsEncountered("npc:witch");
        SaveManager.AddNpcsEncountered("npc:blacksmith");
        SaveManager.AddNpcsEncountered("npc:lucas");
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        storyTellerInteraction.Interacted -= OnStoryTellerInteracted;
        blackSmithInteraction.Interacted -= OnBlackSmithInteracted;
        witchInteraction.Interacted -= OnWitchInteracted;
        QuestManager.QuestCompleted -= UpdateQuest;
    }

    private void UpdateQuest(Quest quest5)
    {
        questFlag = 2;
        var quests = new[] { quest2, quest3, quest4 };
        isQuestActive = false;

        foreach (var quest in quests)
        {
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

        if (questFlag == 3)
        {
            QuestManager.Add(mainQuest);
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