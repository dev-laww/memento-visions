using Godot;

namespace Game.Components;

[GlobalClass]
[Tool]
public partial class ContinuousDamageHitBox : HitBox
{
    private Timer timer;

    [Export]
    private float damageInterval = 0.5f;

    public override void _Ready()
    {
        timer = new Timer
        {
            WaitTime = damageInterval,
            Autostart = true
        };
        timer.Timeout += DealDamage;
        AddChild(timer);
    }

    private void DealDamage()
    {
        foreach (var area in GetOverlappingAreas())
        {
            if (!area.IsInGroup("HurtBox")) continue;
            
            var hurtBox = (HurtBox) area;

            hurtBox.ReceiveDamage(Damage);
        }
    }
}