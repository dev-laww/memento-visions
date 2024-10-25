using System;
using Game.Components;
using Game.Components.Area;
using Game.Components.Movement;
using Godot;
using GodotUtilities;
using GodotUtilities.Logic;


namespace Game.Entities.Player;

[Scene]
[GlobalClass]
public partial class Player : CharacterBody2D
{
    [Export]
    public float DashStaminaCost { get; set; } = 10f;

    [Node]
    public StatsManager statsManager;

    [Node]
    private HurtBox hurtBox;

    [Node]
    private AnimationPlayer animations;

    [Node]
    private Velocity velocity;

    private string MoveDirection => GetMoveDirection();

    private Vector2 lastMoveDirection = Vector2.Down;
    private Vector2 DashVelocity { get; set; }
    private bool CanDash { get; set; } = true;
    private bool Dashing { get; set; }
    private bool CanMove { get; set; } = true;

    private DelegateStateMachine StateMachine = new();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        statsManager.StatsChanged += StatChangeHandler;
        hurtBox.DamageReceived += (damage) => GD.Print($"Player received {damage} damage.");

        StateMachine.AddStates(Idle);
        StateMachine.AddStates(Walk);
        StateMachine.AddStates(Dash, EnterDash, ExitDash);
        StateMachine.AddStates(Attack, EnterAttack, ExitAttack);
        StateMachine.SetInitialState(Idle);

        velocity.Accelerating += () => StateMachine.ChangeState(Walk);
        velocity.Decelerating += () => StateMachine.ChangeState(Idle);
        velocity.DashEnded += HandleTransition;
    }

    public override void _PhysicsProcess(double delta)
    {
        StateMachine.Update();

        if (!CanMove) return;

        var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (!input.IsZeroApprox())
            lastMoveDirection = input;

        if (Dashing) return;

        velocity.Accelerate(input);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("attack") && CanMove && !Dashing) StateMachine.ChangeState(Attack);

        if (!@event.IsActionPressed("dash") || !CanDash || !CanMove) return;

        StateMachine.ChangeState(Dash);
    }

    private string GetMoveDirection()
    {
        if (lastMoveDirection == Vector2.Zero) return "front";

        if (Math.Abs(lastMoveDirection.X) > Math.Abs(lastMoveDirection.Y))
            return lastMoveDirection.X > 0 ? "right" : "left";

        return lastMoveDirection.Y < 0 ? "back" : "front";
    }

    private void StatChangeHandler(float value, StatsType stat)
    {
        CanDash = stat switch
        {
            StatsType.Stamina => value >= DashStaminaCost,
            _ => CanDash
        };
    }

    private void Idle() => animations.Play($"idle_{MoveDirection}");

    private void Walk() => animations.Play($"walk_{MoveDirection}");

    private void EnterDash()
    {
        statsManager.ConsumeStamina(DashStaminaCost);
        Dashing = true;
    }

    private void Dash()
    {
        velocity.Dash(lastMoveDirection);
        animations.Play($"walk_{MoveDirection}");
    }

    private void ExitDash() => Dashing = false;

    private void EnterAttack() => CanMove = false;

    private async void Attack()
    {
        velocity.Stop();
        animations.Play($"gun_{MoveDirection}");

        await ToSignal(animations, "animation_finished");

        HandleTransition();
    }

    private void ExitAttack() => CanMove = true;

    private void HandleTransition()
    {
        var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (input.Length() > 0) StateMachine.ChangeState(Walk);
        else StateMachine.ChangeState(Idle);
    }
}