using Game.Common;
using Game.Common.Utilities;
using Game.Components;
using Game.Autoload;
using Game.Data;
using Godot;
using GodotUtilities;
using System.CommandLine.IO;
using Game.UI.Screens;
using Game.UI.Common;
using System.Linq;
using Game.Common.Models;

namespace Game.Entities;

[Scene]
public partial class Player : Entity
{
    private readonly StringName[] ANIMATION_STATES = ["idle", "move"];

    private const int MAX_COMBO = 3;

    [Node] private HurtBox hurtBox;
    [Node] private AnimationTree animationTree;
    [Node] private Timer comboResetTimer;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private AudioStreamPlayer2D sfxDash;
    [Node] private HBoxContainer dashContainer;
    [Node] private DashIndicator dashIndicator;

    [Node] public VelocityManager VelocityManager;
    [Node] public WeaponManager WeaponManager;
    [Node] public InputManager InputManager;
    [Node] public Marker2D Center;

    public string LastFacedDirection => VelocityManager.GetEightDirectionString();

    private AnimationNodeStateMachinePlayback playback;
    private int combo = 1;
    private bool isAttacking;

    public override void _EnterTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Unregister(this);

        SaveManager.SetLevel(StatsManager.Level);
        SaveManager.SetExperience(StatsManager.Experience);

        SaveManager.Save();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        comboResetTimer.Timeout += OnComboReset;
        StatsManager.LevelUp += OnLevelUp;
        StatsManager.StatIncreased += OnStatIncreased;
        VelocityManager.Dashed += OnDash;

        StateMachine.AddStates(Normal);
        StateMachine.AddStates(Attack, EnterAttack, ExitAttack);
        StateMachine.AddStates(Dash, EnterDash, ExitDash);

        StateMachine.SetInitialState(Normal);

        var data = SaveManager.Data;

        StatsManager.SetLevel(data?.Level ?? 1);
        StatsManager.SetExperience(data?.Experience ?? 0);

        dashIndicator.Value = 100;

        for (var i = 1; i < VelocityManager.TimesCanDash; i++)
        {
            dashContainer.AddChild(dashIndicator.Duplicate());
        }

    }

    public override void OnProcess(double delta)
    {
        var collision = VelocityManager.MoveAndCollide();

        if (collision is not null && collision.GetCollider() is RigidBody2D body)
        {
            var direction = VelocityManager.Velocity.Normalized();

            body.ApplyCentralImpulse(direction * (1000 / body.Mass));
        }

        UpdateNormalBlendPositions();
    }

    protected override void Die(DeathInfo info)
    {
        var deathScreen = resourcePreloader.InstanceSceneOrNull<Death>();

        if (deathScreen is null)
        {
            Log.Error("Failed to load death screen");
            return;
        }

        GameManager.CurrentScene.AddChild(deathScreen);

        base.Die(info);
    }

    public Vector2 GetPlayerDirection()
    {
        return InputManager.GetVector8();
    }


    #region States

    public void Normal()
    {
        ProcessMovement();

        var attacking = InputManager.IsActionJustPressed("attack");
        var dashing = InputManager.IsActionJustPressed("dash");
        var quickUse = InputManager.IsActionJustPressed("quick_use");

        if (attacking && WeaponManager.CanAttack) StateMachine.ChangeState(Attack);
        if (dashing && VelocityManager.CanDash(VelocityManager.LastFacedDirection)) StateMachine.ChangeState(Dash);
        if (quickUse) PlayerInventoryManager.UseQuickSlotItem();
    }

    public async void Attack()
    {
        ProcessMovement();

        await ToSignal(animationTree, "animation_finished");

        StateMachine.ChangeState(Normal);
    }

    public void EnterAttack()
    {
        isAttacking = true;
        UpdateAttackBlendPositions();

        WeaponManager.Animate(combo);
        WeaponManager.PlayAttackSound();
    }

    public void ExitAttack()
    {
        combo = combo >= MAX_COMBO ? 1 : combo + 1;
        comboResetTimer.Start();
        isAttacking = false;
    }

    private void EnterDash()
    {
        sfxDash.Play();
        VelocityManager.Dash(VelocityManager.LastFacedDirection);

        hurtBox.Disable();
    }

    private void Dash()
    {
        if (VelocityManager.IsDashing) return;

        StateMachine.ChangeState(Normal);
    }

    private void ExitDash()
    {
        hurtBox.Enable();
    }

    #endregion

    #region Utilities

    private void UpdateNormalBlendPositions()
    {
        if (InputManager.IsLocked) return;

        var mousePosition = InputManager.GetGlobalMousePosition();
        var directionToMouse = (mousePosition - GlobalPosition).Normalized();

        foreach (var animation in ANIMATION_STATES)
        {
            animationTree.Set($"parameters/{animation}/blend_position", directionToMouse);
        }
    }

    private void UpdateAttackBlendPositions()
    {
        var mousePosition = InputManager.GetGlobalMousePosition();
        var directionToMouse = (mousePosition - GlobalPosition).Normalized();

        animationTree.Set("parameters/sword_and_dagger/blend_position", directionToMouse);
        animationTree.Set("parameters/whip/blend_position", directionToMouse);
        WeaponManager.SetBlendPosition(directionToMouse);
    }

    private void ProcessMovement()
    {
        var inputDirection = InputManager.GetVector8();

        if (inputDirection.IsZeroApprox())
        {
            VelocityManager.Decelerate();
        }
        else
        {
            VelocityManager.Accelerate(inputDirection);
        }
    }

    #endregion

    #region SignalListeners

    private void OnLevelUp(float level)
    {
        Log.Debug($"Player leveled up to {level}");
        GetTree().CreateTimer(0.3f).Timeout += () =>
        {
            var text = FloatingTextManager.SpawnFloatingText(new FloatingTextManager.FloatingTextSpawnArgs
            {
                Text = $"Level up to {level}!",
                Position = GlobalPosition,
                Parent = GetParent(),
                Color = new Color(1f, 1f, 0.5f),
                Deferred = true
            });

            text.Finished += text.QueueFree;
        };

        SaveManager.SetLevel(StatsManager.Level);
        SaveManager.SetExperience(StatsManager.Experience);
    }

    private void OnComboReset()
    {
        combo = 1;
        Log.Debug("Combo reset");
    }

    private void OnStatIncreased(float amount, StatsType type)
    {
        if (type != StatsType.Experience) return;

        var text = FloatingTextManager.SpawnFloatingText(new FloatingTextManager.FloatingTextSpawnArgs
        {
            Text = $"+{Mathf.RoundToInt(amount)} XP",
            Position = GlobalPosition,
            Parent = GetParent(),
            Color = Colors.Gray,
            Deferred = true
        });

        text.Finished += text.QueueFree;
    }

    private void OnDash(Vector2 _)
    {
        var indicators = dashContainer.GetChildrenOfType<DashIndicator>().Where(x => !x.Running).ToArray();

        if (indicators.Length == 0) return;

        var indicator = indicators.Last();

        indicator.Start(VelocityManager.DashCoolDown);
    }

    #endregion

    #region Commands

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

    [Command(Name = "levelup", Description = "Increases the player's level")]
    private void LevelUp(float level = 1) => StatsManager.IncreaseLevel(level);

    [Command(Name = "apply", Description = "Applies a status effect to the player")]
    private void ApplyStatusEffect(string statusEffectId)
    {
        var statusEffect = StatusEffectRegistry.Get(statusEffectId);

        if (statusEffect is null)
        {
            DeveloperConsole.Console.Error.WriteLine("Status effect not found");
            return;
        }

        StatsManager.AddStatusEffect(statusEffect);
    }

    #endregion
}
