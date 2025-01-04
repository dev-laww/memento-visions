using Godot;

namespace Game.Resources;

public enum Variant
{
    Dagger,
    Sword,
    Gun,
    Whip
}

[Tool]
[GlobalClass]
public partial class Weapon : Item
{
    [Export] public Variant Variant { get; set; } = Variant.Dagger;
    [Export] public SpriteFrames Animations { get; set; }
    [Export] public AudioStream AttackSound { get; set; }
    [Export] public AudioStream HitSound { get; set; }

    protected override bool IsStackable() => false;
    protected override Type GetItemType() => Type.Weapon;
    protected override void SetItemType(Type value) { }
}