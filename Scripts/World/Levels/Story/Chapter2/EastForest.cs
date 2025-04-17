using Godot;
using System;
using GodotUtilities;
using Game.Data;
using Game.Autoload;

namespace Game.World;

[Scene]
public partial class EastForest : BaseLevel
{
    [Node] private Node2D enemy;
    [Node] private ScreenMarker screenMarker, witchMarker;


    private Quest quest = QuestRegistry.Get("quest:restoring_balance");

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

        QuestManager.QuestCompleted += OnQuestUpdated;
    }
    private void OnQuestUpdated(Quest quest)
    {

        if (quest.Id != "quest:restoring_balance" || !quest.Objectives[1].Completed) return;
        QuestManager.QuestUpdated -= OnQuestUpdated;
        screenMarker.Toggle(true);

    }
    public override void _ExitTree()
    {
        base._ExitTree();
        enemy.QueueFree();
    }
}