using Game.Globals;
using Godot;

namespace Game.Entities.Enemies;

[Tool]
[Icon("res://assets/icons/enemy.svg")]
public abstract partial class Enemy : Entity
{
    public enum EnemyType
    {
        Common,
        Boss
    }

    [Export] public string Id;
    [Export] private EnemyType Type;

    public override void _Ready()
    {
        if (Engine.IsEditorHint()) return;

        base._Ready();

        EnemyManager.Register(this);
    }

    protected override void Die(Entity entity)
    {
        if (Engine.IsEditorHint()) return;

        base.Die(entity);

        EnemyManager.Unregister(this);
    }
}