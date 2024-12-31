using System;
using Game.Components.Managers;
using Game.Components.Movement;
using DialogueManagerRuntime;
using Game.Entities;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;
using Range = Godot.Range;

namespace Game.Entities.Character;

[Scene]
public partial class Escort : Entity
{
    [Node] private Velocity velocity;
    [Node] private AnimationPlayer Animation;
    [Node] private Area2D Range;
    [Node] private Area2D ChaseRange;
    [Export] private Resource DialogResource;
    private bool inRange ;

public string Name = "npc";
    private Vector2 lastMoveDirection = Vector2.Down;


    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        
       ChaseRange.BodyEntered += body =>
        {
            if (body is Player.Player player)
            {
                
                inRange = false;
                StateMachine.ChangeState(Follow);
            }
        };
       Range.BodyExited += body =>
        {
            if (body is Player.Player player)
            {
                inRange = false;
                StateMachine.ChangeState(Follow);
            }
        };
       
        Range.BodyEntered += body =>
        {
            if (body is Player.Player player)
            {
                inRange = true;
                StateMachine.ChangeState(Idle);
            }
        };
        
        ChaseRange.BodyExited += body =>
        {
            if (body is Player.Player player)
            {
                inRange = true;
                StateMachine.ChangeState(Idle);
            }
        };
        
        StatsManager.StatsDecreased += StatDecrease;
        StateMachine.AddStates(Idle);
        StateMachine.AddStates(Follow);
        StateMachine.AddStates(Hurt);
        StateMachine.SetInitialState(Idle);
    }

    public override void _PhysicsProcess(double delta) => StateMachine.Update();

    private void Idle()
    {
        velocity.Decelerate();
        PlayDirectionalAnimation("idle");

        var player = this.GetPlayer();
        if (!inRange && player != null)
        {
            StateMachine.ChangeState(Follow);
        }
    }

    private void Follow()
    {
        var player = this.GetPlayer();
        if (player == null)
        {
            StateMachine.ChangeState(Idle);
            return;
        }

        var distance = GlobalPosition.DistanceTo(player.GlobalPosition);

        if (inRange)
        {
            StateMachine.ChangeState(Idle);
            return;
        }

        var direction = (player.GlobalPosition - GlobalPosition).Normalized();
        velocity.Accelerate(direction);
        UpdateLastMoveDirection(direction);
        PlayDirectionalAnimation("walk");
    }

    private async void Hurt()
    {
        PlayDirectionalAnimation("hurt");
        await ToSignal(Animation, "animation_finished");

        StateMachine.ChangeState(Idle);
    }

    private void StatDecrease(float value, StatsType stat)
    {
        if (stat != StatsType.Health) return;

        // StateMachine.ChangeState(Hurt);
    }

    private void UpdateLastMoveDirection(Vector2 direction)
    {
        if (direction.Length() == 0) return; 
        lastMoveDirection = direction;
    }

    private void PlayDirectionalAnimation(string baseAnimation)
    {
        var moveDirection = GetMoveDirection();
        Animation.Play($"{baseAnimation}_{moveDirection}");
    }

     private string GetMoveDirection()
    {
        if (lastMoveDirection == Vector2.Zero)
            return "Down"; 

        if (Math.Abs(lastMoveDirection.X) > Math.Abs(lastMoveDirection.Y))
            return lastMoveDirection.X > 0 ? "right" : "left";

        return lastMoveDirection.Y < 0 ? "back" : "front";
    }

}