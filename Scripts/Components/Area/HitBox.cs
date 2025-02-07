using Game.Utils.Battle;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class HitBox : Area2D
{
    [Export] public Attack.Type Type { get; set; } = Attack.Type.Physical;

    [Export] public float Damage;

    public Attack Attack => Attack.Create(Damage, Type, Owner as Node2D);

    public override void _Ready()
    {
        CollisionLayer = 1 << 10;
        CollisionMask = 1 << 11;
        NotifyPropertyListChanged();
    }
}