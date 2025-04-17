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
        transitionArea.Toggle(false);

        QuestManager.QuestCompleted += OnQuestUpdated;
    }
    private void OnQuestUpdated(Quest tits)
    {

        if (quest.Id != "quest:restoring_balance" || !quest.Objectives[0].Completed) return;
        QuestManager.QuestUpdated -= OnQuestUpdated;
        transitionArea.Toggle(true);
        screenMarker.Toggle(true);

    }

    public void EnableTransitionArea()
    {
        transitionArea.Toggle(true);
        
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        enemy.QueueFree();
    }
}