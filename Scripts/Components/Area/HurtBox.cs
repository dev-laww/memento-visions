using System.Collections.Generic;
using System.Linq;
using Game.Common.Extensions;
using Game.Entities;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass, Icon("res://assets/icons/hurtbox.svg")]
public partial class HurtBox : Area2D
{
    [Export]
    private StatsManager StatsManager
    {
        get => statsManager;
        set
        {
            statsManager = value;
            UpdateConfigurationWarnings();
        }
    }

    private StatsManager statsManager;

    public override void _EnterTree()
    {
        if (GetChildren().OfType<CollisionShape2D>().Any()) return;

        this.EditorAddChild(new CollisionShape2D { Name = "CollisionShape2D", DebugColor = new Color(1f, 0.3f, 0.4f, 0.4f) });
    }

    public override void _Ready()
    {
        AddToGroup("HurtBox");
        AreaEntered += OnHurtBoxAreaEntered;
        CollisionLayer = 1 << 11;
        CollisionMask = 1 << 10;
        NotifyPropertyListChanged();
    }

    private void OnHurtBoxAreaEntered(Area2D area)
    {
        if (area is not HitBox hitBox) return;

        var owner = hitBox.HitboxOwner ?? hitBox.Owner;

        if (owner == StatsManager.Owner) return;

        statsManager.ReceiveAttack(hitBox.Attack);
        hitBox.EmitHit();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (statsManager == null)
            warnings.Add("StatsManager is not set.");

        if (Owner as Entity is null)
            warnings.Add("Owner is not an Entity.");

        return [.. warnings];
    }
}