using System.Collections;
using Game.Autoload;
using Game.Components;
using Game.Data;
using Game.Utils;
using Game.Utils.Battle;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Entities;

[Scene]
public partial class ShockWave : Node2D
{
    public const float ATTACK_WIDTH = 64f;
    public const float ATTACK_LENGTH = 250f;
    private const int ELECTRIC_SHOCKS_COUNT = 5;

    [Node] private AnimationTree animationTree;

    private Vector2 origin;
    private Vector2 destination;
    private Aghon aghon;
    private AnimationNodeStateMachinePlayback playback;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public void Start(Vector2 origin, Vector2 destination, Aghon aghon)
    {
        this.origin = origin;
        this.destination = destination;
        this.aghon = aghon;
        playback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        Rotation = (destination - origin).Angle();
        GlobalPosition = origin;

        new DamageFactory.HitBoxBuilder(origin)
           .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", IsGuaranteed = true })
           .SetDamage(aghon.StatsManager.Damage * .8f)
           .SetRotation((destination - origin).Angle())
           .SetDamageType(Attack.Type.Magical)
           .SetShapeOffset(new Vector2(ATTACK_LENGTH / 2, 0))
           .SetShape(new RectangleShape2D { Size = new Vector2(ATTACK_LENGTH, ATTACK_WIDTH) })
           .SetOwner(aghon)
           .Build();

        playback.Travel("loop");
        GameCamera.Shake(0.5f);

        HitBox lastHitBox = null;

        for (int i = 0; i < ELECTRIC_SHOCKS_COUNT; i++)
        {
            var hitbox = new DamageFactory.HitBoxBuilder(origin)
                .AddStatusEffectToPool(new StatusEffect.Info { Id = "electrocute", Chance = 0.5f })
                .SetDamage(aghon.StatsManager.Damage * .5f)
                .SetDelay(i * 0.5f)
                .SetDamageType(Attack.Type.Magical)
                .SetRotation((destination - origin).Angle())
                .SetShapeOffset(new Vector2(ATTACK_LENGTH / 2, 0))
                .SetShape(new RectangleShape2D { Size = new Vector2(ATTACK_LENGTH, ATTACK_WIDTH) })
                .SetOwner(aghon)
                .Build();

            lastHitBox = hitbox;
        }

        if (lastHitBox is null) return;

        lastHitBox.TreeExiting += () =>
        {
            animationTree.AnimationFinished += _ => QueueFree();
            playback.Travel("exit");
        };
    }
}

