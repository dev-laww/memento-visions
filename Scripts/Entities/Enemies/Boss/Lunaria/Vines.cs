using Godot;
using Game.Common.Extensions;
using Game.Data;
using Game.Utils;
using Game.Utils.Extensions;
using GodotUtilities;
using System.Collections.Generic;
using Game.Utils.Battle;

namespace Game.Entities;

[Scene]
public partial class Vines : Node2D
{
    [Node] private AnimatedSprite2D animatedSprite2D;
    [Node] private AnimationPlayer animationPlayer;

    private int spawnCount = 0;
    private const int MaxSpawns = 3;
    private Vector2 playerPosition;
    private Lunaria lunaria;

    private readonly List<Vector2> spawnOffsets = new()
    {
        //use RNG
        new Vector2(-40, -12),
        new Vector2(40, -12),
        new Vector2(0, -12)
    };

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        lunaria = GetTree().Root.GetFirstChildOrNull<Lunaria>();
        if (lunaria == null) return;
        SpawnNextVine();
    }

    private void SpawnNextVine()
    {
        if (spawnCount >= MaxSpawns)
        {
            QueueFree();
            return;
        }

        playerPosition = this.GetPlayer()?.GlobalPosition ?? GlobalPosition;
        var offset = spawnOffsets[spawnCount];
        var telegraphOrigin = playerPosition + offset;
        GlobalPosition = telegraphOrigin;

        var direction = lunaria.GlobalPosition - playerPosition;
        var canvas = this.GetTelegraphCanvas();
        var isRight = spawnCount == 0 ? false : (spawnCount == 1 ? true : direction.X > 0);

        animationPlayer.Play("spawn");

        var telegraph = new TelegraphFactory.LineTelegraphBuilder(canvas, telegraphOrigin)
            .SetDestitnation(telegraphOrigin + (isRight ? Vector2.Left : Vector2.Right) * 60)
            .SetWidth(16f)
            .SetDelay(0.5f)
            .Build();

        telegraph.TreeExiting += () => OnTelegraphFinished(!isRight);
        spawnCount++;
    }

    private void OnTelegraphFinished(bool isRight)
    {
        if (lunaria == null) return;

        animationPlayer.AnimationFinished -= OnAnimationFinished;
        animationPlayer.AnimationFinished += OnAnimationFinished;

        animationPlayer.Play(isRight ? "attack_right" : "attack_left");
        new DamageFactory.HitBoxBuilder(GlobalPosition)
            .AddStatusEffectToPool(new StatusEffect.Info { Id = "slow", IsGuaranteed = true })
            .SetDelay(0.4f)
            .SetDamage(lunaria.StatsManager.Damage * 1.7f)
            .SetDamageType(Attack.Type.Magical)
            .SetShape(new RectangleShape2D { Size = new Vector2(100, 16) })
            .SetOwner(lunaria)
            .Build();
    }

    private void OnAnimationFinished(StringName anim)
    {
        if (anim == "attack_left" || anim == "attack_right")
        {
            animationPlayer.Play("despawn");
        }
        else if (anim == "despawn")
        {
            animationPlayer.AnimationFinished -= OnAnimationFinished;

            if (spawnCount < MaxSpawns)
            {
                SpawnNextVine();
            }
            else
            {
                QueueFree();
            }
        }
    }
}