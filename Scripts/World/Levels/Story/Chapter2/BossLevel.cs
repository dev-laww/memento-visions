using Godot;
using System;
using Game.Autoload;
using Game.Data;
using Game.Entities;
using Game.World.Puzzle;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class BossLevel : BaseLevel
{
    [Node] private Node2D enemy;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private Marker2D cinematicPosition1, lunariaSpawnPoint;
    [Node] private ScreenMarker screenMarker;
    [Node] private LeverManager leverManager;
    [Node] private Marker2D chestMarker;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        screenMarker.Toggle(false);
        QuestManager.QuestUpdated += OnQuestUpdated;
        leverManager.IsComplete += OnPuzzleComplete;
        StartCutscene();
    }

    private void OnQuestUpdated(Quest quest)
    {
        if (quest.Id != "quest:engkanto_wrath" || !quest.Objectives[0].Completed) return;
        CinematicManager.StartCinematic();
        MoveCameraTo(lunariaSpawnPoint.GlobalPosition, 1f,
            () => { CinematicManager.EndCinematic(); });
        SpawnLunaria();
        QuestManager.QuestUpdated -= OnQuestUpdated;

        if (quest.Id != "quest:engkanto_wrath" || !quest.Objectives[1].Completed) return;
        screenMarker.Toggle(true);
    }


    public void StartCutscene()
    {
        CinematicManager.StartCinematic();
        MoveCameraTo(cinematicPosition1.GlobalPosition, 2.5f,
            () =>
            {
                MoveCameraTo(lunariaSpawnPoint.GlobalPosition, 2.5f,
                    () => { CinematicManager.EndCinematic(); });
            });
    }

    public static void MoveCameraTo(Vector2 position, float duration, Action onComplete = null)
    {
        GameCamera.SetTargetPositionOverride(position);
        var timer = GameCamera.Instance.GetTree().CreateTimer(duration);
        timer.Timeout += () => { onComplete?.Invoke(); };
    }

    private void OnPuzzleComplete()
    {
        GD.Print("Puzzle complete.....");
        var chest = resourcePreloader.InstanceSceneOrNull<Chest>();
        chest.GlobalPosition = chestMarker.GlobalPosition;
        chest.SetDrops(LootTableRegistry.Get("8.tres"));
        AddChild(chest);
    }
    

    public void SpawnLunaria()
    {
        SaveManager.AddEnemyDetails("enemy:lunaria");
        var lunaria = resourcePreloader.InstanceSceneOrNull<Lunaria>();
        lunaria.GlobalPosition = lunariaSpawnPoint.GlobalPosition;
        enemy.AddChild(lunaria);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        QuestManager.QuestUpdated -= OnQuestUpdated;
        enemy.QueueFree();
    }
}