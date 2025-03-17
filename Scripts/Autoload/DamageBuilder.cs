using Game.Common.Extensions;
using Game.Components;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Autoload;


[Scene]
public partial class DamageBuilder : Autoload<DamageBuilder>
{
    [Node] private ResourcePreloader resourcePreloader;

    private static Damage damageComponent; // TODO: make base class for all damage types

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static DamageBuilder Circle()
    {
        damageComponent = Instance.resourcePreloader.InstanceSceneOrNull<CircleDamage>();

        return Instance;
    }

    public static DamageBuilder Line()
    {
        damageComponent = Instance.resourcePreloader.InstanceSceneOrNull<LineDamage>();

        return Instance;
    }

    public DamageBuilder WithRadius(float radius)
    {
        damageComponent?.SetRadius(radius);

        return this;
    }

    public DamageBuilder WithPosition(Vector2 position)
    {
        damageComponent?.SetGlobalPosition(position);

        return this;
    }

    public DamageBuilder WithDamage(float damage)
    {
        damageComponent?.SetDamage(damage);

        return this;
    }

    public DamageBuilder WithWaitTime(float waitTime)
    {
        damageComponent?.SetDuration(waitTime);

        return this;
    }

    public DamageBuilder WithOwner(Entity owner)
    {
        damageComponent?.SetOwner(owner);

        return this;
    }

    public Damage Build()
    {
        var canvas = GetTree().Root.GetFirstChildOrNull<TelegraphCanvas>();

        if (canvas == null)
        {
            GD.PrintErr("TelegraphCanvas not found.");
            return null;
        }

        if (damageComponent == null)
        {
            GD.PrintErr("Create a circle damage before building.");
            return null;
        }

        var damage = damageComponent;

        damage.Start(canvas);

        damageComponent = null;

        return damage;
    }
}
