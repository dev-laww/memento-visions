using Godot;
using Game.Autoload;
using Game.Components;
using Game.Data;
using Game.Entities;
using Game.World;
using Game.World.Puzzle;
using GodotUtilities;

namespace Game.World;

[Scene]
public partial class EverfieldCity : BaseLevel
{
    [Node] private Entity kevin;
    [Node] private TransitionArea transitionArea;
    [Node] private TorchPuzzleManager torchSequence;
    [Node] private PressurePlate plate, plate2;
    [Node] private Marker2D chestMarker, chestMarker2;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private ScreenMarker kevinMarker,jeepMarker,mannyMarker;
    [Node] private Node2D enemy;
    private Quest quest = QuestRegistry.Get("quest:aswang_hunt");
    private Quest quest1 = QuestRegistry.Get("quest:whispers_in_intramuros");
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        QuestManager.QuestUpdated += OnQuestUpdated;
        torchSequence.PuzzleSolved += OnPuzzleSolved;
        plate.Activated += OnPlatePressed;
        plate2.Activated += OnPlatePressed;
        transitionArea.Toggle(false); 
        jeepMarker.Toggle(false);
        kevinMarker.Toggle(false);

    }
    
    private void OnPuzzleSolved()
    {
        var chest = resourcePreloader.InstanceSceneOrNull<Chest>();
        chest.GlobalPosition = chestMarker.GlobalPosition;
        chest.SetDrops(LootTableRegistry.Get("9.tres"));
        AddChild(chest);
    }

    public void EnableTransitionArea()
    {
        kevinMarker.Toggle(false);
        transitionArea.Toggle(true);
        jeepMarker.Toggle(true);
    }
    private void OnPlatePressed()
    {

        if (plate.isActive && plate2.isActive)
        {
            var chest2 = resourcePreloader.InstanceSceneOrNull<Chest>();
            chest2.GlobalPosition = chestMarker2.GlobalPosition;
            chest2.SetDrops(LootTableRegistry.Get("9.tres"));
            AddChild(chest2);
        }
    }

    private void OnQuestUpdated(Quest updatedQuest)
    {
        if (quest.Objectives[0].Completed && !kevin.Visible)
        {
            kevin.Visible = true;
            kevinMarker.Toggle(true);
            QuestManager.QuestUpdated -= OnQuestUpdated;
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
    public override void _ExitTree()
    {
        GD.Print("Exiting tree");
        enemy.QueueFree();
        plate.Activated -= OnPlatePressed;
        plate2.Activated -= OnPlatePressed;
        torchSequence.PuzzleSolved -= OnPuzzleSolved;
        QuestManager.QuestUpdated -= OnQuestUpdated;
        base._ExitTree();
    }
}