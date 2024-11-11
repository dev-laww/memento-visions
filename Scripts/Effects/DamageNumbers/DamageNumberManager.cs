using System.Linq;
using Game.Battle;
using Game.Components.Area;
using Game.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Effects.DamageNumbers;

// TODO: make this health numbers and add regen numbers

[Tool]
[Scene]
public partial class DamageNumberManager : Node2D
{
    [Export]
    private HurtBox hurtBox
    {
        get => box;
        set
        {
            box = value;
            UpdateConfigurationWarnings();
        }
    }

    [Node]
    private Marker2D numberSpawn;

    [Node]
    private ResourcePreloader resourcePreloader;

    [Node]
    private Timer timer;

    private HurtBox box;
    private DamageNumber number => numberSpawn.GetChildrenOfType<DamageNumber>().FirstOrDefault();
    private float damage;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        hurtBox.AttackReceived += OnAttackReceived;
        timer.Timeout += OnTimerTimeout;
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new System.Collections.Generic.List<string>();

        if (box == null)
            warnings.Add("HurtBox is not set.");

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