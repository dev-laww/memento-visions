using Godot;

namespace Game.Components.Area;

[Tool]
[GlobalClass]
public partial class ContinuousDamageHitBox : HitBox
{
    private Timer timer;

    [Export] private float damageInterval = 0.5f;

    public override void _Ready()
    {
        base._Ready();

        if (Engine.IsEditorHint()) return;

        timer = new Timer
        {
            WaitTime = damageInterval,
            Autostart = false
        };
        timer.Timeout += DealDamage;
        AddChild(timer);

        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    private void DealDamage()
    {
        foreach (var area in GetOverlappingAreas())
        {
            if (!area.IsInGroup("HurtBox")) continue;

            var hurtBox = (HurtBox)area;

            hurtBox.ReceiveAttack(this);
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (!area.IsInGroup("HurtBox")) return;

        timer.Start();
    }

    private void OnAreaExited(Area2D area)
    {
        if (!area.IsInGroup("HurtBox")) return;

        timer.Stop();
    }
}