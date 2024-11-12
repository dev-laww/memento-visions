using System.Linq;
using Game.Battle;
using Game.Components;
using Game.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Effects.HealthNumbers;

// TODO: make this health numbers and add regen numbers

[Tool]
[Scene]
public partial class HealthNumberManager : Node2D
{
    [Export]
    private StatsManager statsManager
    {
        get => manager;
        set
        {
            manager = value;
            UpdateConfigurationWarnings();
        }
    }

    [Node]
    private Marker2D numberSpawn;

    [Node]
    private ResourcePreloader resourcePreloader;

    [Node]
    private Timer timer;

    private StatsManager manager;
    private DamageNumber number => numberSpawn.GetChildrenOfType<DamageNumber>().FirstOrDefault();
    private float damage;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        if (statsManager != null)
            statsManager.AttackReceived += OnAttackReceived;
        timer.Timeout += OnTimerTimeout;
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();

        if (manager == null)
            warnings.Add("StatsManager is not set.");

        return warnings.ToArray();
    }

    private void OnAttackReceived(float dmg, Attack.Type type, bool critical)
    {
        timer.Reset();
        damage += dmg;

        if (number == null)
        {
            var scene = resourcePreloader.GetResource<PackedScene>("number");
            var num = scene.Instantiate<DamageNumber>();
            numberSpawn.AddChild(num);
        }

        number!.Text = $"{damage}";
        number!.DamageType = type;

        if (critical)
        {
            number.Critical();
            return;
        }

        number.Animate();
    }

    private async void OnTimerTimeout()
    {
        damage = 0;

        if (number == null) return;

        await number.Exit();
    }
}