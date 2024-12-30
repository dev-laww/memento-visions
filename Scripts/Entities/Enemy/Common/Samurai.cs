using Game.Components.Managers;
using Game.Components.Movement;
using Game.Entities;
using Game.Quests;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Enemy.Common;

[Scene]
public partial class Samurai : Entity
{
    [Signal] public delegate void EnemyDiedEventHandler(string enemyName);

    [Node] private Velocity velocity;
    [Node] private Area2D Range;
    [Node] private AnimationPlayer Animation;
    
    [Export] private string Name;
    

    private bool inRange;
    private bool attacking;
    private string attackDirection;
    
    
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
         
        base._Ready();
        StatsManager.StatsDepleted += OnStatsDepleted;
        Range.BodyEntered += body =>
        {
            inRange = true;

            if (!attacking)
                StateMachine.ChangeState(Attack);
        };
        Range.BodyExited += body =>
        {
            inRange = false;

            if (!attacking)
                StateMachine.ChangeState(Walk);
        };
        StatsManager.StatsDecreased += StatDecrease;
        StateMachine.AddStates(Idle);
        StateMachine.AddStates(Walk);
        StateMachine.AddStates(Hurt);
        StateMachine.AddStates(Attack, EnterAttacking, ExitAttacking);
        StateMachine.SetInitialState(Walk);
    }

    public override void _PhysicsProcess(double delta) => StateMachine.Update();


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