using Godot;
using System;
using Game.Autoload;
using Game.Data;
using Game.Entities;
using GodotUtilities;

namespace Game.World;

[Scene]

public partial class BossLevel : BaseLevel
{
    [Node] private Node2D enemy;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private Marker2D cinematicPosition1, cinematicPosition2,lunariaSpawnPoint;
    private Quest quest = QuestRegistry.Get("res://resources/quests/chapter_2/engkanto_wrath.tres");
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        QuestManager.QuestUpdated += OnQuestUpdated;
        StartCutscene(cinematicPosition1.GlobalPosition);
        StartCutscene(cinematicPosition2.GlobalPosition);
    }
    
    private void OnQuestUpdated(Quest quest)
    {
        if (this.quest.Objectives[0].Completed)
        {
            SpawnLunaria();
            QuestManager.QuestUpdated -= OnQuestUpdated;
        }
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
    
    public void SpawnLunaria()
    {
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