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
    [Node] private Entity Chief2;
    [Node] private TransitionArea TransitionArea;
    [Node] private TorchPuzzleManager TorchSequence;
    [Node] private PressurePlate Plate, Plate2;
    [Node] private Chest Chest,Chest2;
    [Node] private ScreenMarker kevinMarker,jeepMarker;
    private bool plate1, plate2;
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
        TorchSequence.PuzzleSolved += OnPuzzleSolved;
        Plate.Activated += OnPlatePressed;
        Plate2.Activated += OnPlatePressed;
        TransitionArea.Toggle(false); 
        jeepMarker.Toggle(false);
        kevinMarker.Toggle(false);

    }
    
    private void OnPuzzleSolved()
    {
      Chest.Visible = true; 
      
    }

    public void EnableTransitionArea()
    {
        kevinMarker.Toggle(false);
        TransitionArea.Toggle(true);
        jeepMarker.Toggle(true);
    }
    private void OnPlatePressed()
    {
        if (Plate.isActive && Plate2.isActive)
        {
            Chest2.Visible = true; 
        }
    }

    private void OnQuestUpdated(Quest updatedQuest)
    {
        if (quest.Objectives[0].Completed && !Chief2.Visible)
        {
            Chief2.Visible = true;
            kevinMarker.Toggle(true);
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