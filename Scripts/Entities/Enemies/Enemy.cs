using Game.Globals;
using Godot;

namespace Game.Entities.Enemies;

public abstract partial class Enemy : Entity
{
    public enum EnemyType
    {
        Common,
        Boss
    }

    [Export] private EnemyType Type;

    public override void OnReady()
    {
        EnemyManager.Register(this);
    }

    protected override void Die(Entity entity)
    {
        if (Engine.IsEditorHint()) return;

        EnemyManager.Unregister(this);

        base.Die(entity);
    }

    public override string ToString() => $"<Enemy ({Id})>";
}