using System;
using Game.Common;
using Game.Common.Utilities;
using Game.Components;
using Game.Autoload;
using Game.Data;
using Godot;
using GodotUtilities;


namespace Game.Entities;

[Scene]
public partial class Player : Entity
{
    [Node] private HurtBox hurtBox;
    [Node] private AnimationPlayer animations;

    [Node] public InventoryManager InventoryManager;
    [Node] public VelocityManager VelocityManager;
    [Node] public WeaponManager WeaponManager;
    [Node] public QuestManager QuestManager;
    [Node] public InputManager InputManager;
    [Node] private GpuParticles2D trail;

    private Vector2 inputDirection;

    public string LastFacedDirection => VelocityManager.GetFourDirectionString();

    public override void _EnterTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Register(this);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

        WireNodes();
    }

    public override void OnReady()
    {
        if (Engine.IsEditorHint()) return;

        StateMachine.AddStates(Idle);
        StateMachine.AddStates(Walk);
        StateMachine.AddStates(Dash, EnterDash, ExitDash);
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
        animations.Play($"idle/{LastFacedDirection}");

        if (inputDirection.IsZeroApprox())
        {
            VelocityManager.Decelerate();
            return;
        }

        StateMachine.ChangeState(Walk);
    }

    private void Walk()
    {
        animations.Play($"walk/{LastFacedDirection}");

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
        animations.Play($"walk/{LastFacedDirection}");

        trail.ShowBehindParent = VelocityManager.LastFacedDirection.Y > 0;
        trail.Emitting = true;
    }

    private void Dash()
    {
        if (VelocityManager.IsDashing) return;

        StateMachine.ChangeState(inputDirection.IsZeroApprox() ? Idle : Walk);
    }

    private void ExitDash()
    {
        trail.Emitting = false;
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


    [Command(Name = "heal", Description = "Adds health to the player")]
    private void AddHealth(float value = 100)
    {
        StatsManager.Heal(value);

        Log.Debug($"Health increased by {value}");
        Log.Debug($"Current health: {StatsManager.Health}");
    }

    [Command(Name = "damage", Description = "Damages the player")]
    private void Damage(float value = 10)
    {
        StatsManager.TakeDamage(value);

        Log.Debug($"Health decreased by {value}");
        Log.Debug($"Current health: {StatsManager.Health}");
    }

    private void OnLevelUp(float level)
    {
        Log.Debug($"Player leveled up to {level}");
        var text = FloatingTextManager.SpawnFloatingText(new FloatingTextManager.FloatingTextSpawnArgs
        {
            Text = $"Level up to {level}!",
            Position = GlobalPosition,
            Parent = GetParent(),
            Color = new Color(1f, 1f, 0.5f),
        });
        text.Finished += text.QueueFree;
    }

    [Command(Name = "levelup", Description = "Increases the player's level")]
    private void LevelUp(float level = 1) => StatsManager.IncreaseLevel(level);
}