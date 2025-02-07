using Godot;

namespace Game.Entities;

public abstract partial class Enemy : Entity
{
    public enum EnemyType
    {
        Common,
        Boss
    }

    [Export] private EnemyType Type;

    public override string ToString() => $"<Enemy ({Id})>";
}