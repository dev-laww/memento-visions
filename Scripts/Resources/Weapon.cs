using Godot;

namespace Game.Resources;

public enum WeaponType
{
    Dagger,
    Sword,
    Gun,
    Whip,
}

[GlobalClass, Icon("res://assets/icons/weapon.svg")]
public partial class Weapon : Item
{
    [Export] public WeaponType Type;
}