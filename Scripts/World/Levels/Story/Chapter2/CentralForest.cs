using Godot;
using System;
using Game.Autoload;
using Game.Entities;
using GodotUtilities;
using Game.Data;
using DialogueManagerRuntime;
using Game.Components;

namespace Game.World;
[Scene]
public partial class CentralForest : BaseLevel
{
    [Node] private Node2D enemy;
    [Node] private StoryTeller storyTeller;
    [Node] private ScreenMarker screenMarker, pageMarker;
    [Node] private AnimationPlayer animationPlayer;

    private Quest quest = QuestRegistry.Get("quest:the_missing_anchor");

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        screenMarker.Toggle(false);
        pageMarker.Toggle(false);
        ShowDialogue();
        QuestManager.QuestUpdated += OnQuestUpdated;
    }

    private void OnQuestUpdated(Quest quest)
    {
        if (quest.Id == "quest:the_missing_anchor" && quest.Objectives[0].Completed)
        {
            pageMarker.Toggle(true);
            GD.Print("triggers");
            QuestManager.QuestUpdated -= OnQuestUpdated;
        }


    }

    public void StartCinematic()
    {
        CinematicManager.StartCinematic();
        GameCamera.SetTargetPositionOverride(storyTeller.GlobalPosition);
        var timer = GameCamera.Instance.GetTree().CreateTimer(2.5f);
        timer.Timeout += () =>
        {
            GameCamera.SetTargetPositionOverride(Vector2.Zero);
            CinematicManager.EndCinematic();
        };
    }

    private static void ShowDialogue()
    {
        var dialogue = ResourceLoader.Load<Resource>("res://resources/dialogues/chapter_2/2.6.dialogue");
        DialogueManager.ShowDialogueBalloon(dialogue);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        enemy.QueueFree();
    }
}