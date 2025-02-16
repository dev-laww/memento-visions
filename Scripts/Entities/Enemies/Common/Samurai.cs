using Game.Components;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Samurai : Enemy
{
    [Node] private VelocityManager VelocityManager;
    [Node] private Area2D Range;
    [Node] private AnimationPlayer Animation;
    [Node] private DropManager DropManager;
    [Node] private PathFindManager PathFindManager;

    private bool inRange;
    private bool attacking;
    private string attackDirection;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void OnReady()
    {
        Range.BodyEntered += _ =>
        {
            inRange = true;

            if (!attacking)
                StateMachine.ChangeState(Attack);
        };
        Range.BodyExited += _ =>
        {
            inRange = false;

            if (!attacking)
                StateMachine.ChangeState(Walk);
        };
        StatsManager.StatDecreased += StatDecrease;
        StateMachine.AddStates(Idle);
        StateMachine.AddStates(Walk);
        StateMachine.AddStates(Hurt);
        StateMachine.AddStates(Attack, EnterAttacking, ExitAttacking);
        StateMachine.SetInitialState(Walk);
    }

    public override void OnPhysicsProcess(double delta)
    {
        VelocityManager.ApplyMovement();
    }

    private void Idle()
    {
        Animation.Play("idle");
    }

    private void Walk()
    {
        PathFindManager.SetTargetPosition(this.GetPlayer().GlobalPosition);
        PathFindManager.Follow();
        Animation.Play("walk");
    }

    private void EnterAttacking()
    {
        attacking = true;

        var player = this.GetPlayer();
        attackDirection = player?.GlobalPosition.X > GlobalPosition.X ? "right" : "left";
    }

    private async void Attack()
    {
        Animation.Play($"attack_{attackDirection}");

        await ToSignal(Animation, "animation_finished");

        if (!inRange)
        {
            StateMachine.ChangeState(Walk);
            return;
        }

        StateMachine.ChangeState(Idle);
    }

    private void ExitAttacking() => attacking = false;

    private async void Hurt()
    {
        Animation.Play("hurt");
        await ToSignal(Animation, "animation_finished");

        if (inRange)
        {
            StateMachine.ChangeState(Idle);
            return;
        }

        StateMachine.ChangeState(Walk);
    }

    private void StatDecrease(float value, StatsType stat)
    {
        if (stat != StatsType.Health || attacking) return;

        StateMachine.ChangeState(Hurt);
    }
}