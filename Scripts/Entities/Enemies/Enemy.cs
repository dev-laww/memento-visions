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
        EnemyManager.Register(new SpawnInfo(this));
    }

    protected override void Die(DeathInfo info)
    {
        if (Engine.IsEditorHint()) return;

        EnemyManager.Unregister(info);

        base.Die(info);
    }

    public override string ToString() => $"<Enemy ({Id})>";
}