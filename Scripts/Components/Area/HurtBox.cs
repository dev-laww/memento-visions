using System.Collections.Generic;
using System.Linq;
using Game.Common.Extensions;
using Game.Entities;
using Godot;
using GodotUtilities;

namespace Game.Components;

[Tool]
[Scene]
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

    private CollisionShape2D shape;
    private StatsManager statsManager;

    public override void _EnterTree()
    {
        if (this.GetChildrenOfType<CollisionShape2D>().Any() && Engine.IsEditorHint()) return;

        this.EditorAddChild(new CollisionShape2D { Name = "CollisionShape2D", DebugColor = new Color(1f, 0.3f, 0.4f, 0.4f) });
    }

    public override void _Ready()
    {
        AreaEntered += OnHurtBoxAreaEntered;
        CollisionLayer = 1 << 11;
        CollisionMask = 1 << 10;
        NotifyPropertyListChanged();

        if (Engine.IsEditorHint()) return;

        shape = this.GetChildrenOfType<CollisionShape2D>().First();
    }

    private void OnHurtBoxAreaEntered(Area2D area)
    {
        if (area is not HitBox hitBox) return;

        var owner = hitBox.HitBoxOwner ?? hitBox.Owner;
        var isBothEnemy = owner.IsInGroup("Enemy") && StatsManager.Owner.IsInGroup("Enemy");

        if (owner == StatsManager.Owner || isBothEnemy) return;

        statsManager.ReceiveAttack(hitBox.Attack);
        hitBox.EmitHit();
    }

    public void Disable(float duration = -1)
    {
        shape.Disabled = true;

        if (duration > 0)
            GetTree().CreateTimer(duration).Timeout += () => shape.Disabled = false;
    }

    public void Enable() => shape.Disabled = false;

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