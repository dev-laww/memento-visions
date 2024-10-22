using System;
using Game.Components;
using Game.Components.Area;
using Game.Logic.States;
using Game.Utils.Extensions;
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
    public AnimatedSprite2D sprites;

    [Node]
    public StatsManager statsManager;

    [Node]
    private HurtBox hurtBox;
    
    [Node]
    private AnimationPlayer WeaponAnimationPlayer;

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
        StateMachine.AddStates(Dash, EnterDash);
        StateMachine.AddStates(Attack, EnterAttack, ExitAttack);
        StateMachine.SetInitialState(Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        StateMachine.Update();
        Velocity = Input.GetVector("move_left", "move_right", "move_up", "move_down") * statsManager.Speed;

        if (Dashing)
            Velocity = DashVelocity;

        Velocity = Velocity.SnapToGrid();
        Velocity = CanMove ? Velocity : Vector2.Zero;

        if (Velocity.Length() > 0 && CanMove)
            lastMoveDirection = Velocity.Normalized();

        MoveAndSlide();

        if (Input.IsActionJustPressed("attack")) StateMachine.ChangeState(Attack);

        if (!Input.IsActionJustPressed("dash") || !CanDash || !CanMove) return;

        statsManager.ConsumeStamina(DashStaminaCost);
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

    private void Idle()
    {
        sprites.Play($"idle_{MoveDirection}");
        if (Velocity.Length() > 0) StateMachine.ChangeState(Walk);
    }

    private void Walk()
    {
        sprites.Play($"walk_{MoveDirection}");
        if (Velocity.IsZeroApprox()) StateMachine.ChangeState(Idle);
    }

    private void EnterDash() => Dashing = true;

    private void Dash()
    {
        DashVelocity = lastMoveDirection * statsManager.Speed * 10;

        var tween = CreateTween();
        tween.SetParallel().TweenProperty(this, "DashVelocity", Vector2.Zero, 0.1f);
        tween.SetParallel().TweenProperty(this, "Dashing", false, 0.1f);
        tween.Finished += () =>
        {
            if (Velocity.IsZeroApprox())
            {
                StateMachine.ChangeState(Idle);
                return;
            }
            
            StateMachine.ChangeState(Walk);
        };
    }

    private void EnterAttack() => CanMove = false;

    private async void Attack()
    {
        sprites.Play($"attack_{MoveDirection}");
        WeaponAnimationPlayer.Play($"dagger_{MoveDirection}");

        await ToSignal(sprites, "animation_finished");
        await ToSignal(WeaponAnimationPlayer, "animation_finished");

        StateMachine.ChangeState(Idle);
    }

    private void ExitAttack() => CanMove = true;
}