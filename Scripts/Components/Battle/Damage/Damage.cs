using Game.Entities;
using Game.Utils.Battle;
using Godot;
using GodotUtilities;

namespace Game.Components;

[Scene]
[GlobalClass]
public abstract partial class Damage : Node2D
{
    public abstract void SetOwner(Entity owner);
    public abstract void SetDamage(float damage);
    public abstract void SetDuration(float duration);
    public abstract void SetType(Attack.Type type);
    public abstract void SetRadius(float radius);
    public abstract void Start(TelegraphCanvas canvas);
}

