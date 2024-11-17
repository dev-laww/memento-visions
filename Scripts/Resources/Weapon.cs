using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass]
public partial class Weapon : Item
{
    public enum Variant
    {
        Dagger,
        Sword,
        Gun,
        Whip
    }

    [Export]
    private Variant Type { get; set; } = Variant.Dagger;

    [Export]
    private SpriteFrames Animations { get; set; }

    [Export]
    private AudioStream AttackSound { get; set; }

    [Export]
    private AudioStream HitSound { get; set; }

    public Weapon()
    {
        StackSize = (int)StackSizes.Unstackable;
        NotifyPropertyListChanged();
    }

    protected override bool IsStackable() => false;
    protected override void SetStackable(bool value) => NotifyPropertyListChanged();
}