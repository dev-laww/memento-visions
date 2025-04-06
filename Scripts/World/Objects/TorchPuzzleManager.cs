using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Components;
using GodotUtilities;

namespace Game.World.Objects;

[Scene]
public partial class TorchPuzzleManager : Node
{
    [Node] private Interaction interaction;
    [Export] private float displaySequenceDelay = 1.0f;
    [Export] private float resetDelay = 5.0f;
    [Export] private int sequenceLength = 4;
    
    [Signal] public delegate void PuzzleSolvedEventHandler();
    [Signal] public delegate void PuzzleFailedEventHandler();

    private StreetLight[] torches;
    private int[] correctSequence;
    private readonly List<int> playerSequence = new();
    private Timer resetTimer;
    
    private enum PuzzleState { Inactive, ShowingSequence, Active }
    private PuzzleState currentState;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated && !Engine.IsEditorHint())
            WireNodes();
    }

    public override void _Ready()
    {
        interaction.Interacted += StartPuzzle;
        InitializeTorches();
        ValidateTorches();
        GenerateNewSequence();
    }

    private void InitializeTorches()
    {
        torches = GetChildren()
            .Where(child => child is StreetLight)
            .Cast<StreetLight>()
            .ToArray();

        foreach (var (torch, index) in torches.WithIndex())
            torch.TorchLit += () => OnTorchLit(index);
    }

    private void ValidateTorches()
    {
        if (torches.Length == 0)
            GD.PushError("No StreetLight nodes found in children!");
        
        if (sequenceLength > torches.Length)
            GD.PushError("Sequence length exceeds available torches!");
    }
    

    public void StartPuzzle()
    {
        if (currentState != PuzzleState.Inactive) return;
        
        ResetPuzzleState();
        _ = ShowSequenceAsync();
    }

    private async Task ShowSequenceAsync()
    {
        currentState = PuzzleState.ShowingSequence;
        
        foreach (var index in correctSequence)
        {
            ResetAllTorches();
            torches[index].LightUp(true);
            await ToSignal(GetTree().CreateTimer(displaySequenceDelay), "timeout");
        }
        
        ResetAllTorches();
        currentState = PuzzleState.Active;
        resetTimer.Start(resetDelay);
    }

    private void GenerateNewSequence()
    {
        var indices = Enumerable.Range(0, torches.Length).ToArray();
        indices.Shuffle();
        correctSequence = indices.Take(sequenceLength).ToArray();
    }

    private void OnTorchLit(int torchIndex)
    {
        if (currentState != PuzzleState.Active) return;

        resetTimer.Start(resetDelay);
        playerSequence.Add(torchIndex);

        if (!ValidateCurrentStep())
        {
            OnPuzzleFailed();
            return;
        }

        if (playerSequence.Count == correctSequence.Length)
            OnPuzzleSolved();
    }

    private bool ValidateCurrentStep()
    {
        var currentStep = playerSequence.Count - 1;
        return playerSequence[currentStep] == correctSequence[currentStep];
    }

    private void OnPuzzleSolved()
    {
        currentState = PuzzleState.Inactive;
        resetTimer.Stop();
        EmitSignalPuzzleSolved();
        GD.Print("Puzzle solved!");
    }

    private void OnPuzzleFailed()
    {
        currentState = PuzzleState.Inactive;
        resetTimer.Stop();
        ResetAllTorches();
        EmitSignalPuzzleFailed();
        GD.Print("Puzzle failed!");
    }

    private void ResetPuzzleState()
    {
        playerSequence.Clear();
        GenerateNewSequence();
        ResetAllTorches();
    }

    private void ResetAllTorches()
    {
        foreach (var torch in torches)
            torch.ResetLight();
    }
}

public static class ArrayExtensions
{
    private static readonly Random Rng = new();

    public static void Shuffle<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = Rng.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }
}

public static class LinqExtensions
{
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}