using System;
using System.Linq;
using Game.Components.Managers;
using Game.Components.Area;
using Godot;
using GodotUtilities;


namespace Game.Entities.Characters;

[Scene]
public partial class Player : Entity
{
    [Node] private HurtBox hurtBox;
    [Node] private AnimationPlayer animations;
    [Node] public VelocityManager VelocityManager;
    [Node] private Node2D hitBoxes;

    private Vector2 inputDirection;

    private string LastFacedDirection
    {
        get
        {
            var lastMoveDirection = VelocityManager.LastFacedDirection;

            if (lastMoveDirection == Vector2.Zero) return "front";

            if (Math.Abs(lastMoveDirection.X) > Math.Abs(lastMoveDirection.Y))
                return lastMoveDirection.X > 0 ? "right" : "left";

            return lastMoveDirection.Y < 0 ? "back" : "front";
        }
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        hitBoxes.GetChildrenOfType<HitBox>().ToList().ForEach(box =>
        {
            box.Damage = StatsManager.Damage;

            box.NotifyPropertyListChanged();
        });

        StatsManager.StatChanged += (value, stat) =>
        {
            if (stat != StatsType.Damage) return;

            hitBoxes.GetChildrenOfType<HitBox>().ToList().ForEach(box =>
            {
                box.Damage = value;
                box.NotifyPropertyListChanged();
            });
        };

        if (Engine.IsEditorHint()) return;

        StateMachine.AddStates(Idle);
        StateMachine.AddStates(Walk);
        StateMachine.AddStates(Dash, EnterDash, ExitDash);
        StateMachine.AddStates(Attack, EnterAttack, ExitAttack);
        StateMachine.SetInitialState(Idle);
    }

    public override void OnPhysicsProcess(double delta)
    {
        ProcessInput();
        StateMachine.Update();
        VelocityManager.ApplyMovement();
    }

    private void Idle()
    {
        animations.Play($"idle_{LastFacedDirection}");

        if (inputDirection.IsZeroApprox())
        {
            VelocityManager.Decelerate();
            return;
        }

        StateMachine.ChangeState(Walk);
    }

    private void Walk()
    {
        animations.Play($"walk_{LastFacedDirection}");

        if (inputDirection.IsZeroApprox())
        {
            StateMachine.ChangeState(Idle);
            return;
        }

        VelocityManager.Accelerate(inputDirection);
    }

    private void EnterDash() { }
    private void Dash() { }
    private void ExitDash() { }

    private void EnterAttack() { }
    private void Attack() { }
    private void ExitAttack() { }


    private void ProcessInput()
    {
        inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        // add dash input and attack input
    }
}