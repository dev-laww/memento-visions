using Game.Common.Extensions;
using Game.Components;
using Game.Data;
using Game.Utils;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class Spear : Node2D
{
    private const float SPEAR_RADIUS = 50f;
    private const int ELECTRIC_SHOCKS_COUNT = 5;

    [Node] private Sprite2D sprite2D;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        var playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        var canvas = this.GetTelegraphCanvas();
        var aghon = GetTree().Root.GetFirstChildOrNull<Aghon>();
        var direction = (aghon.GlobalPosition - playerPosition).Normalized();

        GlobalPosition = playerPosition;
        Rotation = direction.X > 0 ? 140f : 0f; // flip the actual sprite instead of rotating the node

        var telegraph = new TelegraphFactory.CircleTelegraphBuilder(canvas, playerPosition)
            .SetRadius(SPEAR_RADIUS)
            .SetDelay(.5f)
            .Build();

        telegraph.TreeExiting += OnTelegraphFinished;
    }


    private void OnTelegraphFinished()
    {
        var aghon = GetTree().Root.GetFirstChildOrNull<Aghon>();

        new DamageFactory.HitBoxBuilder(GlobalPosition)
            .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", IsGuaranteed = true })
            .SetDamage(aghon.StatsManager.Damage)
            .SetShape(new CircleShape2D { Radius = SPEAR_RADIUS })
            .SetOwner(aghon)
            .Build();

        HitBox lastHitBox = null;

        for (int i = 0; i < ELECTRIC_SHOCKS_COUNT; i++)
        {
            var hitbox = new DamageFactory.HitBoxBuilder(GlobalPosition)
                .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", Chance = 0.5f })
                .SetDamage(aghon.StatsManager.Damage * .3f)
                .SetShape(new CircleShape2D { Radius = SPEAR_RADIUS })
                .SetDelay(.5f + i * .2f)
                .SetOwner(aghon)
                .Build();

            lastHitBox = hitbox;
        }

        lastHitBox.TreeExiting += QueueFree;
    }
}

