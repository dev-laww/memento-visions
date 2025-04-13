using Godot;
using System;
using Game.World.Puzzle;
using GodotUtilities;
using DialogueManagerRuntime;
using Game.Autoload;
using Game.UI.Screens;

namespace Game.World.Levels.Chapter2;

[Scene]
public partial class SmallVille : BaseLevel
{
    [Node] private ScreenMarker witchMarker;
    [Node] private TorchPuzzleManager torchSequence;
    [Node] private Chest chest;
    [Node] private Node2D enemy;
    [Node] private Marker2D tikbalangPosition;
    public bool IsWitchInteracted = false;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
    
    public override void _Ready()
    {
        base._Ready();
        torchSequence.PuzzleSolved += OnPuzzleSolved;
        witchMarker.Toggle(false);
        ShowDialogue();
    }
    
    private void OnPuzzleSolved()
    {
        chest.Visible = true;
    }
    
    public void ToggleWitchMarker(bool isActive)
    {
        witchMarker.Toggle(isActive);
    }
    private static void ShowDialogue()
    {
        var dialogue = ResourceLoader.Load<Resource>("res://resources/dialogues/chapter_2/2.0.dialogue");
        DialogueManager.ShowDialogueBalloon(dialogue);
    }

    public void StartCinematic()
    {
        CinematicManager.StartCinematic();
        GameCamera.SetTargetPositionOverride(tikbalangPosition.GlobalPosition);
        var timer = GameCamera.Instance.GetTree().CreateTimer(2.5f);
        timer.Timeout += () =>
        {
            GameCamera.SetTargetPositionOverride(Vector2.Zero);
            CinematicManager.EndCinematic();
        };
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        torchSequence.PuzzleSolved -= OnPuzzleSolved;
        enemy.QueueFree();
        
    }
    
}

