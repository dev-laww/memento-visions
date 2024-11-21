using Godot;

namespace Game.Utils.Logic.States;

[Tool]
[GlobalClass]
public abstract partial class State : Node
{
    public abstract void Enter();
    public abstract void Update(double delta);
    public abstract void PhysicsUpdate(double delta);
    public abstract void Exit();

    public override string[] _GetConfigurationWarnings()
    {
        return GetParent() is StateMachine
            ? System.Array.Empty<string>()
            : new[] { "State must be a child of StateMachine" };
    }
}