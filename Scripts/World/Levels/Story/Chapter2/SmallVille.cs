using Godot;
using System;
using Game.World.Puzzle;
using GodotUtilities;
using DialogueManagerRuntime;
using Game.Autoload;
using Game.Common.Models;
using Game.UI.Screens;
using Game.Data;
using Quest = Game.Data.Quest;

namespace Game.World.Levels.Chapter2;

[Scene]
public partial class SmallVille : BaseLevel
{
    [Node] private ScreenMarker witchMarker;
    [Node] private TorchPuzzleManager torchSequence;
    [Node] private Node2D enemy;
    [Node] private Marker2D tikbalangPosition , chestMarker;
    [Node] private ScreenMarker screenMarker;
    [Node] private ResourcePreloader resourcePreloader;
    public bool IsWitchInteracted = false;
    private Quest quest = QuestRegistry.Get("quest:echoes_of_void");
    private Quest mainQuest = QuestRegistry.Get("quest:roots_of_the_unseen");

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
    
    public override void _Ready()
    {
        base._Ready();
        torchSequence.PuzzleSolved += OnPuzzleSolved;
        witchMarker.Toggle(false);
        screenMarker.Toggle(false);
        ShowDialogue();
        mainQuest.CompleteObjective(0);

        QuestManager.QuestUpdated += OnQuestUpdated;
    }
    
    private void OnPuzzleSolved()
    {
        var chest = resourcePreloader.InstanceSceneOrNull<Chest>();
        chest.GlobalPosition = chestMarker.GlobalPosition;
        chest.SetDrops(LootTableRegistry.Get("3.tres"));
        AddChild(chest);
    }
    
    public void ToggleWitchMarker(bool isActive)
    {
        witchMarker.Toggle(isActive);
    }
    private static void ShowDialogue()
    {
        var dialogue = ResourceLoader.Load<Resource>("res://resources/dialogues/chapter_2/2.0.dialogue");
        DialogueManager.ShowDialogueBalloon(dialogue);
    }

    public void StartCinematic()
    {
        SaveManager.AddEnemyDetails("enemy:tikbalang");
        CinematicManager.StartCinematic();
        GameCamera.SetTargetPositionOverride(tikbalangPosition.GlobalPosition);
        var timer = GameCamera.Instance.GetTree().CreateTimer(2.5f);
        timer.Timeout += () =>
        {
            GameCamera.SetTargetPositionOverride(Vector2.Zero);
            CinematicManager.EndCinematic();
        };
        mainQuest.CompleteObjective(1);
    }

    private void OnQuestUpdated(Quest quest)
    {
        if (quest.Id != "quest:echoes_of_void" || !quest.Objectives[0].Completed) return;

        QuestManager.QuestUpdated -= OnQuestUpdated;
        screenMarker.Toggle(true);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        torchSequence.PuzzleSolved -= OnPuzzleSolved;
        enemy.QueueFree();
        
    }
    
}

