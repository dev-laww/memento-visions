using Game.Logic.States;
using Godot;
using GodotUtilities;

namespace Game.Entities.Player.States;

[Tool]
[Scene]
public partial class Idle : State
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

    public override void Enter()
    {
    }

    public override void Exit() { }

    public override void Update(double delta)
    {
        sprites.Play($"idle_{player.MoveDirection}");
    }

    public override void PhysicsUpdate(double delta)
    {
        if (player.Velocity.Length() > 0)
            stateMachine.ChangeState("walk");
    }
}