using Godot;

namespace Game.Resources;

[GlobalClass, Icon("res://assets/icons/weapon.svg")]
public partial class Weapon : Item
{
    public enum Type
    {
        Dagger,
        Sword,
        Gun,
        Whip,
    }

    [Export] public Type WeaponType;
}