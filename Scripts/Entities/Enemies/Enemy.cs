using Game.Globals;
using Godot;

namespace Game.Entities.Enemies;

public abstract partial class Enemy : Entity
{
    [Export] private string Id;

    public override void _Ready()
    {
        base._Ready();

        EnemyManager.Register(this);
    }

    protected override void Die(Entity entity)
    {
        base.Die(entity);

        EnemyManager.Unregister(this);
    }
}