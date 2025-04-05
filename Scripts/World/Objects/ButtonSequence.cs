using Godot;
using System;
using System.Collections.Generic;
using GodotUtilities;
using Godot.Collections;

namespace Game.World.Objects;

[Scene]
public partial class ButtonSequence : Node2D
{
    [Export] private Array<int> sequenceOrder = new();
    private readonly List<int> inputStack = new();
    private bool iscompleted = false;

    [Signal]
    public delegate void SequenceMatchedEventHandler();

    public override void _Ready()
    {
        if (sequenceOrder.Count != 4 && !Engine.IsEditorHint())
            GD.PushError("SequenceOrder must have exactly 4 elements");
    }

    public void PushInput(int value)
    {
        if (iscompleted) return;
        inputStack.Add(value);
        if (inputStack.Count > 4)
            inputStack.RemoveAt(0);

        GD.Print(string.Join("", inputStack));

        CheckForSequenceMatch();
    }

    private void CheckForSequenceMatch()
    {
        if (inputStack.Count != 4) return;
        
        var targetSequence = new List<int>(sequenceOrder);

        if (SequenceMatches(targetSequence))
        {
            GD.Print("SUCCESS! Sequence matched!");
            EmitSignal(SignalName.SequenceMatched);
            iscompleted = true;
        }
    }

    private bool SequenceMatches(List<int> target)
    {
        for (var i = 0; i < 4; i++)
            if (inputStack[i] != target[i])
                return false;
        return true;
    }
}