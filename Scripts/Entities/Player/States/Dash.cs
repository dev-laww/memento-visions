using Game.Components;
using Game.Logic.States;
using Godot;
using GodotUtilities;

namespace Game.Entities.Player.States;

[Scene]
[Tool]
public partial class Dash : State
{
    [Node]
    private AnimatedSprite2D sprites;

    [Node]
    private StateMachine stateMachine;

    [Node]
    private StatsManager statsManager;

    private Player player;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;
         
        WireNodes();
        player = Owner as Player;
    }

    public override void Enter()
    {
        player.Dashing = true;
    }


    public override void Update(double delta) { }

    public override void PhysicsUpdate(double delta)
    {
        player.DashVelocity = player.lastMoveDirection * statsManager.Speed * 20;

        var tween = CreateTween();
        tween.SetParallel().TweenProperty(player, "DashVelocity", Vector2.Zero, 0.1f);
        tween.SetParallel().TweenProperty(player, "Dashing", false, 0.1f);
        tween.Finished += () => stateMachine.ChangeState("idle");
    }

    public override void Exit() { }
}