using Game.Components.Managers;
using Game.Components.Movement;
using Game.Quests;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;
using GodotUtilities.Logic;

namespace Game.Enemy.Common;

[Scene]
public partial class Samurai : CharacterBody2D
{
    [Node]
    private StatsManager StatsManager;

    [Node]
    private Velocity velocity;

    [Node]
    private Area2D Range;

    [Node]
    private AnimationPlayer Animation;

    [Export]
    private string Name;

    private bool inRange;
    private bool attacking;
    private string attackDirection;
    private DelegateStateMachine stateMachine = new();
    private SlayObjectives SlayObjectives = new();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        StatsManager.StatsDepleted += OnStatsDepleted;
        Range.BodyEntered += body =>
        {
            inRange = true;

            if (!attacking)
                stateMachine.ChangeState(Attack);
        };
        Range.BodyExited += body =>
        {
            inRange = false;

            if (!attacking)
                stateMachine.ChangeState(Walk);
        };
        StatsManager.StatsDecreased += StatDecrease;
        stateMachine.AddStates(Idle);
        stateMachine.AddStates(Walk);
        stateMachine.AddStates(Hurt);
        stateMachine.AddStates(Attack, EnterAttacking, ExitAttacking);
        stateMachine.SetInitialState(Walk);
    }

    public override void _PhysicsProcess(double delta)
    {
        stateMachine.Update();
    }


    private void Idle()
    {
        velocity.Decelerate();
        Animation.Play("idle");
    }

    private void Walk()
    {
        var player = this.GetPlayer();

        var direction = (player.GlobalPosition - GlobalPosition).Normalized();

        velocity.Accelerate(direction);
        Animation.Play("walk");
    }

    private void EnterAttacking()
    {
        attacking = true;

        var player = this.GetPlayer();
        attackDirection = player.GlobalPosition.X > GlobalPosition.X ? "right" : "left";
    }

    private async void Attack()
    {
        velocity.Decelerate();

        Animation.Play($"attack_{attackDirection}");

        await ToSignal(Animation, "animation_finished");

        if (!inRange)
        {
            stateMachine.ChangeState(Walk);
            return;
        }

        stateMachine.ChangeState(Idle);
    }

    private void ExitAttacking()
    {
        attacking = false;
    }

    private async void Hurt()
    {
        Animation.Play("hurt");
        await ToSignal(Animation, "animation_finished");

        if (inRange)
        {
            stateMachine.ChangeState(Idle);
            return;
        }

        stateMachine.ChangeState(Walk);
    }

    private void StatDecrease(float value, StatsType stat)
    {
        if (stat != StatsType.Health || attacking) return;

        stateMachine.ChangeState(Hurt);
    }

    private void OnStatsDepleted(StatsType stat)
    {
        SlayObjectives.OnEnemyDied(Name);
    }
}