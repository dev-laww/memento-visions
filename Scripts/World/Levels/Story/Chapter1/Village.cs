using System;
using Game.Autoload;
using Godot;
using Game.Components;
using Game.Data;
using Game.Entities;
using Game.World.Puzzle;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class Village : BaseLevel
{
    [Node] private TransitionArea transitionArea;
    [Node] private DialogueTrigger dialogueTrigger;
    [Node] private QuestTrigger questTrigger2;
    [Node] private Marker2D chestMarker, chestMarker2;
    [Node] private LeverManager leverManager;
    [Node] private TorchPuzzleManager lightPuzzle;
    [Node] private Entity rudy, mayor;
    [Node] private ScreenMarker mayorMarker;
    [Node] private ScreenMarker rudyMarker;
    [Node] private ScreenMarker screenMarker;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private QuestTrigger rudyQuestTrigger;
    private Quest quest = QuestRegistry.Get("quest:find_mayor");

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated && !Engine.IsEditorHint())
            WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        questTrigger2.Monitoring = false;
        transitionArea.Toggle(false);
        mayorMarker.Toggle(false);
        rudyMarker.Toggle(false);
        screenMarker.Toggle(false);
        rudy.Visible = false;
        rudyQuestTrigger.Monitoring = false;
        leverManager.IsComplete += OnLeverPuzzleComplete;
        lightPuzzle.PuzzleSolved += OnLightPuzzleComplete;
        QuestManager.QuestUpdated += OnQuestUpdated;
    }

    private void OnQuestUpdated(Quest quest)
    {
        if (this.quest.Objectives[1].Completed)
        {
            rudyQuestTrigger.Monitoring = true;
            rudyMarker.Toggle(true);
            QuestManager.QuestUpdated -= OnQuestUpdated;
        }
    }

    private void OnLeverPuzzleComplete()
    {
        var chest = resourcePreloader.InstanceSceneOrNull<Chest>();
        chest.GlobalPosition = chestMarker.GlobalPosition;
        chest.SetDrops(LootTableRegistry.Get("4.tres"));
        AddChild(chest);
    }

    private void OnLightPuzzleComplete()
    {
        var chest2 = resourcePreloader.InstanceSceneOrNull<Chest>();
        chest2.GlobalPosition = chestMarker2.GlobalPosition;
        chest2.SetDrops(LootTableRegistry.Get("7.tres"));
        AddChild(chest2);
    }

    public void SetDialogueTriggerOff()
    {
        dialogueTrigger.Monitoring = false;
    }

    public void setQuestTriggerOn()
    {
        questTrigger2.Monitoring = true;
    }


    public void StartCutscene()
    {
        CinematicManager.StartCinematic();
        MoveCameraTo(rudy.GlobalPosition, 2f, () => { CinematicManager.EndCinematic(); });
    }

    public static void MoveCameraTo(Vector2 position, float duration, Action onComplete = null)
    {
        GameCamera.SetTargetPositionOverride(position);
        var timer = GameCamera.Instance.GetTree().CreateTimer(duration);
        timer.Timeout += () => { onComplete?.Invoke(); };
    }


    public void Spawn()
    {
        for (var i = 0; i < 6; i++)
        {
            var aswang = resourcePreloader.InstanceSceneOrNull<Aswang>();
            aswang.GlobalPosition = rudy.GlobalPosition + new Vector2(0, 100) * MathUtil.RNG.RandDirection();

            AddChild(aswang);
            GD.Print(aswang.GlobalPosition);
        }
    }

    public void SetMayorVisible()
    {
        rudy.Visible = true;
        mayor.QueueFree();
    }
    public override void _ExitTree()
    {
        QuestManager.QuestUpdated -= OnQuestUpdated;
        leverManager.IsComplete -= OnLeverPuzzleComplete;
        lightPuzzle.PuzzleSolved -= OnLightPuzzleComplete;

        base._ExitTree();
    }
}