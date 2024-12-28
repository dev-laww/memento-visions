using System.Collections.Generic;
using System.Linq;
using Game.Utils.Battle;
using Game.Components.Managers;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Effects.HealthNumbers;

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

    [Node]
    private Node2D regenSpawns;

    private StatsManager manager;
    private DamageNumber damage => numberSpawn.GetChildrenOfType<DamageNumber>().FirstOrDefault();
    private float damageReceived;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        if (statsManager != null)
        {
            statsManager.AttackReceived += OnAttackReceived;
            statsManager.StatsIncreased += OnStatsIncreased;
        }

        timer.Timeout += OnTimerTimeout;
    }

    private void OnAttackReceived(float dmg, Attack.Type type, bool critical)
    {
        timer.Reset();
        damageReceived += dmg;

        if (damage == null)
        {
            var num = resourcePreloader.InstanceSceneOrNull<DamageNumber>("damage");
            numberSpawn.AddChild(num);
        }

        damage!.Text = $"{damageReceived}";
        damage!.DamageType = type;

        if (critical)
        {
            damage.Critical();
            return;
        }

        damage.Animate();
    }

    private void OnStatsIncreased(float amount, StatsType type)
    {
        if (type != StatsType.Health) return;

        var num = resourcePreloader.InstanceSceneOrNull<RegenNumber>("regen");
        var spawn = regenSpawns.GetChildrenOfType<Node2D>().ToArray()[MathUtil.RNG.RandiRange(0, 7)];

        num.Text = $"+{amount}";

        spawn.AddChild(num);
    }

    private async void OnTimerTimeout()
    {
        damageReceived = 0;

        if (damage == null) return;

        await damage.Exit();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (manager == null)
            warnings.Add("StatsManager is not set.");


        return warnings.ToArray();
    }
}