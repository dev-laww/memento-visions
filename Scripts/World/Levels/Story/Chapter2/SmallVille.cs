using Godot;
using System;
using Game.World.Puzzle;
using GodotUtilities;
using DialogueManagerRuntime;

namespace Game.World.Levels.Chapter2;

[Scene]
public partial class SmallVille : Node2D
{
    [Node] private ScreenMarker witchMarker;
    [Node] private TorchPuzzleManager torchSequence;
    [Node] private Chest chest;
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
}

