using System;
using Game.Autoload;
using Godot;
using Game.Components;
using Game.Entities;
using Game.World.Puzzle;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class Village : BaseLevel
{
    [Node] private TransitionArea TransitionArea;
    [Node] private DialogueTrigger DialogueTrigger;
    [Node] private QuestTrigger QuestTrigger2;
    [Node] private Chest Chest, Chest2;
    [Node] private LeverManager LeverManager;
    [Node] private TorchPuzzleManager LightPuzzle;
    [Node] private Entity Rudy, Mayor;
    [Node] private ScreenMarker mayorMarker;
    [Node] private ResourcePreloader resourcePreloader;
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated && !Engine.IsEditorHint())
            WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        QuestTrigger2.Monitoring = false;
        TransitionArea.Toggle(false);
        mayorMarker.Toggle(false);
        Rudy.Visible = false;
        LeverManager.IsComplete += OnLeverPuzzleComplete;
        LightPuzzle.PuzzleSolved += OnLightPuzzleComplete;
    }

    private void OnLeverPuzzleComplete()
    {
        Chest.Visible = true;
    }

    private void OnLightPuzzleComplete()
    {
        Chest2.Visible = true;
    }

    public void SetDialogueTriigerOff()
    {
        DialogueTrigger.Monitoring = false;
    }

    public void setQuestTriggerOn()
    {
        QuestTrigger2.Monitoring = true;
    }
    
    
    public void StartCutscene()
    {
        CinematicManager.StartCinematic();
        MoveCameraTo(Rudy.GlobalPosition, 2f, () => { CinematicManager.EndCinematic(); });
    }
    public static void MoveCameraTo(Vector2 position, float duration, Action onComplete = null)
    {
        GameCamera.SetTargetPositionOverride(position);
        var timer = GameCamera.Instance.GetTree().CreateTimer(duration);
        timer.Timeout += () =>
        {
            onComplete?.Invoke();
        };
    }


    public void Spawn()
    {
        for (var i = 0; i < 6; i++)
        {
            var aswang = resourcePreloader.InstanceSceneOrNull<Aswang>();
            aswang.GlobalPosition = Rudy.GlobalPosition + new Vector2(0, 100) * MathUtil.RNG.RandDirection();

            AddChild(aswang);
            GD.Print(aswang.GlobalPosition);
        }
    }
    public void SetMayorVisible()
    {
        Rudy.Visible = true;
        Mayor.Visible = false;
    }
}