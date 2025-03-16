using Game.Common.Extensions;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Autoload;


[Scene]
public partial class DamageBuilder : Autoload<DamageBuilder>
{
    [Node] private ResourcePreloader resourcePreloader;

    private static CircleDamage circleDamage; // TODO: make base class for all damage types

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static DamageBuilder Circle()
    {
        circleDamage = Instance.resourcePreloader.InstanceSceneOrNull<CircleDamage>();

        return Instance;
    }

    public DamageBuilder WithRadius(float radius)
    {
        // circleDamage?.SetRadius(radius);

        return this;
    }

    public DamageBuilder WithPosition(Vector2 position)
    {
        circleDamage?.SetPosition(position);
        // lineDamage?.SetPosition(position);

        return this;
    }

    public DamageBuilder WithDamage(float damage)
    {
        circleDamage?.SetDamage(damage);
        // lineDamage?.SetDamage(damage);

        return this;
    }

    public DamageBuilder WithWaitTime(float waitTime)
    {
        circleDamage?.SetDuration(waitTime);
        // lineDamage?.SetDuration(waitTime);

        return this;
    }

    public DamageBuilder WithOwner(Entity owner)
    {
        circleDamage?.SetOwner(owner);
        // lineDamage?.SetOwner(owner);

        return this;
    }

    public CircleDamage Build()
    {
        var canvas = GetTree().Root.GetFirstChildOrNull<TelegraphCanvas>();

        if (canvas == null)
        {
            GD.PrintErr("TelegraphCanvas not found.");
            return null;
        }

        if (circleDamage == null)
        {
            GD.PrintErr("Create a circle damage before building.");
            return null;
        }

        var damage = circleDamage;

        damage.Start(canvas);

        circleDamage = null;
        // lineDamage = null;

        return damage;
    }
}
