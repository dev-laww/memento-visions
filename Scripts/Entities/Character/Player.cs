using System;
using System.Linq;
using Game.Components.Managers;
using Game.Components.Area;
using Game.Components.Movement;
using Godot;
using GodotUtilities;


namespace Game.Entities.Player;

[Scene]
[GlobalClass]
public partial class Player : Entity
{
    [Export] public float DashStaminaCost { get; set; } = 10f;
    [Node] private HurtBox hurtBox;
    [Node] private AnimationPlayer animations;
    [Node] public Velocity velocity;
    // [Node] private WeaponManager weaponManager;
    // [Node] public InventoryManager Inventory;
    [Node] private Node2D hitBoxes;

    private string MoveDirection => GetMoveDirection();
    private Vector2 lastMoveDirection = Vector2.Down;
    private Vector2 DashVelocity { get; set; }
    private bool Dashing { get; set; }
    private bool CanMove { get; set; } = true;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();

        velocity.Accelerating += () => StateMachine.ChangeState(Walk);
        velocity.Decelerating += () => StateMachine.ChangeState(Idle);
        velocity.DashEnded += HandleTransition;


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


        // TODO: implement weapon unlocking system
        // var weapons = new List<string>()
        // {
        //     "res://resources/weapons/daggers/dagger.tres",
        //     "res://resources/weapons/guns/gun.tres"
        // };
        //
        // weapons.ForEach(path =>
        // {
        //     var res = GD.Load<Weapon>(path);
        //     Inventory.AddItem(res);
        // });
        // Inventory.ChangeWeapon("weapon:dagger");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        StateMachine.Update();

        if (!IsProcessingInput())
        {
            StateMachine.ChangeState(Idle);
            velocity.Decelerate();
            return;
        }

        if (!CanMove || !IsProcessingInput()) return;

        // TODO: Clean movement system, make it handle this
        var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (!input.IsZeroApprox())
            lastMoveDirection = input;

        if (Dashing) return;

        velocity.Accelerate(input);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("attack") && CanMove && !Dashing) StateMachine.ChangeState(Attack);

        if (!@event.IsActionPressed("dash") || !CanMove) return;

        StateMachine.ChangeState(Dash);
    }

    // States
    private void Idle() => animations.Play($"idle_{MoveDirection}");

    private void Walk() => animations.Play($"walk_{MoveDirection}");

    private void EnterDash()
    {
        Dashing = true;
    }

    private void Dash()
    {
        velocity.Dash(lastMoveDirection);
        animations.Play($"walk_{MoveDirection}");
    }

    private void ExitDash() => Dashing = false;

    private void EnterAttack()
    {
        // if (weaponManager.CurrentWeapon == null)
        // {
        //     HandleTransition();
        //     return;
        // }

        CanMove = false;
        velocity.Stop();
    }

    private async void Attack()
    {
        // TODO: make use of animation tree player to handle animation directions
        // switch (weaponManager.CurrentWeaponType)
        // {
        //     case Variant.Gun:
        //         animations.Play($"gun/{MoveDirection}");
        //         break;
        //     case Variant.Dagger:
        //         animations.Play($"dagger/{MoveDirection}");
        //         break;
        //     case Variant.Sword:
        //         animations.Play($"sword/{MoveDirection}");
        //         break;
        //     case Variant.Whip:
        //         animations.Play($"dagger/{MoveDirection}");
        //         break;
        //     default:
        //         GD.PushError("Weapon type not found");
        //         break;
        // }

        // var signal = weaponManager.Animate(MoveDirection);

        await ToSignal(animations, "animation_finished");
        // await signal;

        HandleTransition();
    }

    private void ExitAttack() => CanMove = true;

    // Helpers
    private void HandleTransition()
    {
        var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (input.Length() > 0) StateMachine.ChangeState(Walk);
        else StateMachine.ChangeState(Idle);
    }

    private string GetMoveDirection()
    {
        if (lastMoveDirection == Vector2.Zero) return "front";

        if (Math.Abs(lastMoveDirection.X) > Math.Abs(lastMoveDirection.Y))
            return lastMoveDirection.X > 0 ? "right" : "left";

        return lastMoveDirection.Y < 0 ? "back" : "front";
    }
}