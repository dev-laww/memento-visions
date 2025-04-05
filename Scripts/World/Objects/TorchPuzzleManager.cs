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
    [Node] private Interaction _interaction;
    [Export] private float _displaySequenceDelay = 1.0f;
    [Export] private float _resetDelay = 20.0f;
    [Export] private int _sequenceLength = 4;
    
    [Signal] public delegate void PuzzleSolvedEventHandler();
    [Signal] public delegate void PuzzleFailedEventHandler();

    private StreetLight[] _torches;
    private int[] _correctSequence;
    private readonly List<int> _playerSequence = new();
    private Timer _resetTimer;
    
    private enum PuzzleState { Inactive, ShowingSequence, Active }
    private PuzzleState _currentState;

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated && !Engine.IsEditorHint())
            WireNodes();
    }

    public override void _Ready()
    {
        _interaction.Interacted += StartPuzzle;
        InitializeTorches();
        ValidateTorches();
        InitializeResetTimer();
        GenerateNewSequence();
    }

    private void InitializeTorches()
    {
        _torches = GetChildren()
            .Where(child => child is StreetLight)
            .Cast<StreetLight>()
            .ToArray();

        foreach (var (torch, index) in _torches.WithIndex())
            torch.TorchLit += () => OnTorchLit(index);
    }

    private void ValidateTorches()
    {
        if (_torches.Length == 0)
            GD.PushError("No StreetLight nodes found in children!");
        
        if (_sequenceLength > _torches.Length)
            GD.PushError("Sequence length exceeds available torches!");
    }

    private void InitializeResetTimer()
    {
        _resetTimer = new Timer { OneShot = true };
        AddChild(_resetTimer);
        _resetTimer.Timeout += OnPuzzleFailed;
    }

    public void StartPuzzle()
    {
        if (_currentState != PuzzleState.Inactive) return;
        
        ResetPuzzleState();
        _ = ShowSequenceAsync();
    }

    private async Task ShowSequenceAsync()
    {
        _currentState = PuzzleState.ShowingSequence;
        
        foreach (var index in _correctSequence)
        {
            ResetAllTorches();
            _torches[index].LightUp(true);
            await ToSignal(GetTree().CreateTimer(_displaySequenceDelay), "timeout");
        }
        
        ResetAllTorches();
        _currentState = PuzzleState.Active;
        _resetTimer.Start(_resetDelay);
    }

    private void GenerateNewSequence()
    {
        var indices = Enumerable.Range(0, _torches.Length).ToArray();
        indices.Shuffle();
        _correctSequence = indices.Take(_sequenceLength).ToArray();
    }

    private void OnTorchLit(int torchIndex)
    {
        if (_currentState != PuzzleState.Active) return;

        _resetTimer.Start(_resetDelay); // Reset timeout
        _playerSequence.Add(torchIndex);

        if (!ValidateCurrentStep())
        {
            OnPuzzleFailed();
            return;
        }

        if (_playerSequence.Count == _correctSequence.Length)
            OnPuzzleSolved();
    }

    private bool ValidateCurrentStep()
    {
        var currentStep = _playerSequence.Count - 1;
        return _playerSequence[currentStep] == _correctSequence[currentStep];
    }

    private void OnPuzzleSolved()
    {
        _currentState = PuzzleState.Inactive;
        _resetTimer.Stop();
        EmitSignal(nameof(PuzzleSolved));
        GD.Print("Puzzle solved!");
    }

    private void OnPuzzleFailed()
    {
        _currentState = PuzzleState.Inactive;
        _resetTimer.Stop();
        ResetAllTorches();
        EmitSignal(nameof(PuzzleFailed));
        GD.Print("Puzzle failed!");
    }

    private void ResetPuzzleState()
    {
        _playerSequence.Clear();
        GenerateNewSequence();
        ResetAllTorches();
    }

    private void ResetAllTorches()
    {
        foreach (var torch in _torches)
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