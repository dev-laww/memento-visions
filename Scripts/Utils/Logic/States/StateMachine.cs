using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game.Utils.Logic.States;

[Tool]
[GlobalClass]
public partial class StateMachine : Node
{
    [Export]
    private State initialState;

    public State CurrentState { get; private set; }

    private readonly List<State> states = new();

    public override void _Ready()
    {
        CurrentState = initialState;

        foreach (var child in GetChildren())
        {
            if (!child.IsConnected("script_changed", Callable.From(UpdateConfigurationWarnings)))
                child.ScriptChanged += UpdateConfigurationWarnings;

            if (child is not State state) continue;

            states.Add(state);
        }

        if (Engine.IsEditorHint()) return;

        if (CurrentState == null)
        {
            GD.PushError("Initial state is not set");
            return;
        }

        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void ChangeState(string state)
    {
        var newState = states.Find(s => state.ToLower().Equals(s.Name.ToString().ToLower()));

        if (newState == null)
        {
            GD.PushError($"State {state} not found");
            return;
        }

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        CurrentState?.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        CurrentState?.PhysicsUpdate(delta);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (initialState == null)
            warnings.Add("Initial state is not set");

        warnings.AddRange(
            from state in GetChildren()
            where state is not State
            select $"Child node is not a state: {state.Name}"
        );

        return warnings.ToArray();
    }
}