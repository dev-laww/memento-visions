using Game.Common;
using Game.Common.Utilities;
using Game.Components;
using Game.Autoload;
using Game.Data;
using Godot;
using GodotUtilities;
using System.CommandLine.IO;


namespace Game.Entities;

[Scene]
public partial class Player : Entity
{
    [Node] private HurtBox hurtBox;
    [Node] private AnimationPlayer animations;

    [Node] public VelocityManager VelocityManager;
    [Node] public WeaponManager WeaponManager;
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

        CommandInterpreter.Unregister(this);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated || Engine.IsEditorHint()) return;

        WireNodes();
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