using Godot;
using System;
using GodotUtilities;
using Game.Data;
using Game.Autoload;
using Game.Components;

namespace Game.World;

[Scene]
public partial class EastForest : BaseLevel
{
    [Node] private Node2D enemy;
    [Node] private ScreenMarker screenMarker, witchMarker;
    [Node] private TransitionArea transitionArea;
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        screenMarker.Toggle(false);
        witchMarker.Toggle(true);
        transitionArea.Toggle(false);

        QuestManager.QuestUpdated += OnQuestUpdated;
    }
    private void OnQuestUpdated(Quest quest)
    {

        if (quest.Id != "quest:restoring_balance" || !quest.Objectives[0].Completed) return;
        transitionArea.Toggle(true);
        screenMarker.Toggle(true);
        GD.Print("completed");
        QuestManager.QuestUpdated -= OnQuestUpdated;

    }
    
    public override void _ExitTree()
    {
        base._ExitTree();
        enemy.QueueFree();
        QuestManager.QuestUpdated -= OnQuestUpdated;
    }
}