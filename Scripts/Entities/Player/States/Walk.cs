using Game.Logic.States;
using Godot;
using GodotUtilities;

namespace Game.Entities.Player.States;

[Scene]
[Tool]
public partial class Walk : State
{
    [Node]
    private AnimatedSprite2D sprites;

    [Node]
    private StateMachine stateMachine;

    private Player player;

    public override void _Ready()
    {
        WireNodes();
        player = Owner as Player;
    }

    public override void Enter() { }

    public override void Exit() { }

    public override void Update(double delta)
    {
        sprites.Play($"walk_{player.MoveDirection}");
    }

    public override void PhysicsUpdate(double delta)
    {
        if (player.Velocity.Length() == 0)
            stateMachine.ChangeState("idle");
    }
}