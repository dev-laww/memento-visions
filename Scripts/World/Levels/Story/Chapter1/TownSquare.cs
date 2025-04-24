using System;
using DialogueManagerRuntime;
using Game.Autoload;
using Game.Components;
using Game.Data;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class TownSquare : BaseLevel
{
    [Node] private Entity mayor;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private AnimationPlayer animationPlayer, storyTellerAnimationPlayer;
    [Node] private AudioStreamPlayer2D bossBgm;
    [Node] private AudioStreamPlayer2D chapter1Bgm;
    [Node] private StoryTeller storyTeller;
    [Node] private ScreenMarker screenMarker, mayorMarker;
    [Node] private TransitionArea transitionArea;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            WireNodes();
        }
    }

    public override void _Ready()
    {
        QuestManager.QuestUpdated += OnQuestUpdated;
        screenMarker.Toggle(false);
        mayorMarker.Toggle(true);
        transitionArea.Toggle(false);
    }

    private void OnQuestUpdated(Quest quest)
    {
        if (quest.Id != "quest:testimony" || !quest.Objectives[0].Completed) return;

        QuestManager.QuestUpdated -= OnQuestUpdated;
        StartStoryTellerCutscene();
    }

    public void PlayFadeAnimation()
    {
        mayorMarker.Toggle(false);
        animationPlayer.Play("Fade");
        chapter1Bgm.StreamPaused = true;
        bossBgm.Play();
    }

    private void Spawn()
    {
        mayor.QueueFree();
        SaveManager.AddEnemyDetails("enemy:aghon");
        var aghon = resourcePreloader.InstanceSceneOrNull<Aghon>();
        aghon.GlobalPosition = mayor.GlobalPosition;
        aghon.Death += _ => SaveManager.UnlockFrenzyMode();

        AddChild(aghon);
    }

    private void StartStoryTellerCutscene()
    {
        storyTellerAnimationPlayer.Play("show_story_teller");
        CinematicManager.StartCinematic();
        MoveCameraTo(storyTeller.GlobalPosition, 2f, () =>
        {
            var dialogue = ResourceLoader.Load<Resource>("res://resources/dialogues/chapter_1/1.9.dialogue");
            DialogueManager.DialogueEnded += _ => CinematicManager.EndCinematic();
            DialogueManager.ShowDialogueBalloon(dialogue);
        });
    }

    private static void MoveCameraTo(Vector2 position, float duration, Action onComplete = null)
    {
        GameCamera.SetTargetPositionOverride(position);
        var timer = GameCamera.Instance.GetTree().CreateTimer(duration);
        timer.Timeout += () => { onComplete?.Invoke(); };
    }
}