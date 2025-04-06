using Godot;
using Game.Autoload;
using Game.Components;
using Game.Data;
using Game.Entities;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class EverfieldCity : Node2D
{
    [Node] private Entity Chief2;
    [Node] private TransitionArea TransitionArea;
    private Quest quest = ResourceLoader.Load<Quest>("res://resources/quests/Chapter1/aswang_hunt.tres");
    private Quest quest1 = ResourceLoader.Load<Quest>("res://resources/quests/Prologue/whispers_of_danger.tres");

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        QuestManager.QuestUpdated += OnQuestUpdated;
        TransitionArea.Monitoring = false;
    }

    public void EnableTransitionArea()
    {
        TransitionArea.Monitoring = true;
    }

    private void OnQuestUpdated(Quest updatedQuest)
    {
        if (updatedQuest != quest)
        {
            return;
        }

        if (updatedQuest.Objectives == null || updatedQuest.Objectives.Count == 0)
        {
            GD.PrintErr("Objectives are null or empty!");
            return;
        }

        if (updatedQuest.Objectives[0] == null)
        {
            GD.PrintErr("First objective is null!");
            return;
        }

        if (updatedQuest.Objectives[0].Completed && !Chief2.Visible)
        {
            Chief2.Visible = true;
        }
    }

    public void CompleteQuest(int index)
    {
        if (quest == null)
        {
            GD.PrintErr("Quest not loaded");
            return;
        }

        quest.CompleteObjective(index);
    }

    public void CompleteObjectiveAtIndex(int index)
    {
        if (quest1 == null)
        {
            GD.PrintErr("Quest not loaded");
            return;
        }

        quest1.CompleteObjective(index);
    }
}