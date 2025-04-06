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
        TransitionArea.Monitoring = false;
        TorchSequence.PuzzleSolved += OnPuzzleSolved;
        Plate.Activated += OnPlatePressed;
        Plate2.Activated += OnPlatePressed;

    }
    
    private void OnPuzzleSolved()
    {
      Chest.Visible = true; 
      
    }

    public void EnableTransitionArea()
    {
        TransitionArea.Monitoring = true;
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