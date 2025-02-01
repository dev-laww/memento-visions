using Game.Components.Managers;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities.Enemies.Common;

[Scene]
public partial class Samurai : Enemy
{
    [Node] private VelocityManager VelocityManager;
    [Node] private Area2D Range;
    [Node] private AnimationPlayer Animation;

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

    public override void OnProcess(double delta)
    {
        StateMachine.Update();
        VelocityManager.ApplyMovement();
    }

    private void Idle()
    {
        VelocityManager.Decelerate();
        Animation.Play("idle");
    }

    private void Walk()
    {
        var player = this.GetPlayer();

        var direction = (player?.GlobalPosition - GlobalPosition)?.Normalized() ?? Vector2.Zero;

        VelocityManager.Accelerate(direction);
        Animation.Play("walk");
    }

    private void EnterAttacking()
    {
        attacking = true;

        VelocityManager.MoveAndCollide();

        var player = this.GetPlayer();
        attackDirection = player?.GlobalPosition.X > GlobalPosition.X ? "right" : "left";
    }

    private async void Attack()
    {
        VelocityManager.Decelerate();

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