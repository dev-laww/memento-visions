using Game.Components.Battle;
using Game.Entities.Player;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;

namespace Game.Globals;

// TODO: Cache weapon components
public partial class WeaponManager : Global<WeaponManager>
{
    public static WeaponComponent CurrentWeapon { get; private set; }
    public static AnimationPlayer CurrentAnimationPlayer => CurrentWeapon.AnimationPlayer;
    public static Weapon.Type CurrentWeaponType = Weapon.Type.Dagger;
    private Player Player;


    public override void _Ready()
    {
        Player = this.GetPlayer();
    }

    public static void Equip(Weapon weapon)
    {
        if (Instance.Player == null || weapon.Component.ResourcePath == CurrentWeapon?.SceneFilePath) return;

        var component = weapon.Component.Instantiate<WeaponComponent>();

        CurrentWeapon?.QueueFree();

        Instance.Player.AddChild(component);
        CurrentWeapon = component;
        CurrentWeaponType = weapon.WeaponType;
    }

    public static void Attack(string direction)
    {
        CurrentWeapon?.Animate(direction);
    }

    public static void Unequip()
    {
        CurrentWeapon?.QueueFree();
        CurrentWeapon = null;
        CurrentWeaponType = Weapon.Type.Dagger;
    }
}