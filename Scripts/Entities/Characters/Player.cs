using System;
using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Components;
using Game.Resources;
using Godot;
using GodotUtilities;


namespace Game.Entities;

[Scene]
public partial class Player : Entity
{
    [Node] private HurtBox hurtBox;
    [Node] private AnimationPlayer animations;
    [Node] private Node2D hitBoxes;

    [Node] public InventoryManager InventoryManager;
    [Node] public VelocityManager VelocityManager;
    [Node] public WeaponManager WeaponManager;
    [Node] public QuestManager QuestManager;
    [Node] public InputManager InputManager;

    private Vector2 inputDirection;

    public string LastFacedDirection
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

    public override void _EnterTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Register("heal", AddHealth, "Adds health to the player. Usage: heal [value]");
        CommandInterpreter.Register("damage", Damage, "Removes health from the player. Usage: damage [value]");
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Unregister("heal");
        CommandInterpreter.Unregister("damage");
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

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
        StateMachine.AddStates(Dash, EnterDash);
        StateMachine.AddStates(Attack, EnterAttack);
        StateMachine.SetInitialState(Idle);
    }

    public override void OnProcess(double delta)
    {
        ProcessInput();
    }

    public override void OnPhysicsProcess(double delta)
    {
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

    private void EnterDash()
    {
        if (!VelocityManager.CanDash(VelocityManager.LastFacedDirection)) return;

        VelocityManager.Dash(VelocityManager.LastFacedDirection);
        animations.Play($"walk_{LastFacedDirection}");
    }

    private void Dash()
    {
        if (VelocityManager.IsDashing) return;

        StateMachine.ChangeState(inputDirection.IsZeroApprox() ? Idle : Walk);
    }

    private void EnterAttack()
    {
        if (WeaponManager.CanAttack) return;

        StateMachine.ChangeState(inputDirection.IsZeroApprox() ? Idle : Walk);
    }

    private async void Attack()
    {
        VelocityManager.Decelerate();

        switch (WeaponManager.Weapon.WeaponType)
        {
            case Item.Type.Gun:
                animations.Play($"gun/{LastFacedDirection}");
                break;
            case Item.Type.Dagger:
                animations.Play($"dagger/{LastFacedDirection}");
                break;
            case Item.Type.Sword:
                animations.Play($"sword/{LastFacedDirection}");
                break;
            case Item.Type.Whip:
                animations.Play($"dagger/{LastFacedDirection}");
                break;
            default:
                GD.PushError("Weapon type not found");
                break;
        }

        WeaponManager.Animate();

        await ToSignal(animations, "animation_finished");
        await WeaponManager.AnimationFinished;

        StateMachine.ChangeState(inputDirection.IsZeroApprox() ? Idle : Walk);
    }


    private void ProcessInput()
    {
        inputDirection = InputManager.GetVector();

        var dash = InputManager.IsActionJustPressed("dash");
        var attack = InputManager.IsActionJustPressed("attack");

        if (dash) StateMachine.ChangeState(Dash);
        if (attack) StateMachine.ChangeState(Attack);
    }

    private void AddHealth(float value = 1000)
    {
        StatsManager.Heal(value);

        Log.Debug($"Health increased by {value}");
        Log.Debug($"Current health: {StatsManager.Health}");
    }

    private void Damage(float value = 10)
    {
        StatsManager.TakeDamage(value);

        Log.Debug($"Health decreased by {value}");
        Log.Debug($"Current health: {StatsManager.Health}");
    }
}