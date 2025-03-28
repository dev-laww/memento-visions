using Game.Common;
using Game.Common.Utilities;
using Game.Components;
using Game.Autoload;
using Game.Data;
using Godot;
using GodotUtilities;
using System.CommandLine.IO;
using System.Data;


namespace Game.Entities;

[Scene]
public partial class Player : Entity
{
    private readonly StringName[] ANIMATION_STATES = ["idle", "move"];
    private const string IDLE = "idle";
    private const string MOVE = "move";

    [Node] private HurtBox hurtBox;
    [Node] private AnimationTree animationTree;

    [Node] public VelocityManager VelocityManager;
    [Node] public WeaponManager WeaponManager;
    [Node] public InputManager InputManager;

    private Vector2 inputDirection;
    private AnimationNodeStateMachinePlayback playback;

    public string LastFacedDirection => VelocityManager.GetFourDirectionString();

    public override void _EnterTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;

        CommandInterpreter.Unregister(this);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

        WireNodes();
    }

    public override void OnReady()
    {
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

        StateMachine.AddStates(Idle);
        StateMachine.AddStates(Move);

        StateMachine.SetInitialState(Idle);
    }

    public override void OnProcess(double delta)
    {
        VelocityManager.ApplyMovement();
        UpdateBlendPositions();
    }

    public void Idle()
    {
        playback.Travel(IDLE);

        if (!inputDirection.IsZeroApprox())
        {
            StateMachine.ChangeState(Move);
            return;
        }

        VelocityManager.Decelerate();
    }

    public void Move()
    {
        playback.Travel(MOVE);

        if (InputManager.GetVector().IsZeroApprox())
        {
            StateMachine.ChangeState(Idle);
            return;
        }

        VelocityManager.Accelerate(InputManager.GetVector8());
    }

    private void UpdateBlendPositions()
    {
        foreach (var animation in ANIMATION_STATES)
        {
            animationTree.Set($"parameters/{animation}/blend_position", VelocityManager.LastFacedDirection.Normalized());
        }
    }

    private void ProcessInput()
    {
        inputDirection = InputManager.GetVector8();
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
}