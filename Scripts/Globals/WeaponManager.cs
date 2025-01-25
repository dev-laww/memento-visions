using Game.Components.Battle;
using Game.Entities.Player;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;

namespace Game.Globals;

// TODO: Loading weapon from save file and if player is freed, re-equip weapon
public partial class WeaponManager : Global<WeaponManager>
{
    public static WeaponComponent CurrentWeapon { get; private set; }
    public static AnimationPlayer CurrentAnimationPlayer => CurrentWeapon.AnimationPlayer;
    public static Item CurrentWeaponResource { get; private set; }
    private Player Player;

    public override void _Ready()
    {
        Player = this.GetPlayer();
    }

    public static void Equip(Item weapon)
    {
        if (Instance.Player == null || weapon.Component.ResourcePath == CurrentWeapon?.SceneFilePath) return;

        var component = weapon.Component.Instantiate<WeaponComponent>();

        CurrentWeapon?.QueueFree();

        Instance.Player.AddChild(component);
        CurrentWeapon = component;
        CurrentWeaponResource = weapon;
    }

    public static void Attack(string direction)
    {
        CurrentWeapon?.Animate(direction);
    }

    public static void Unequip()
    {
        CurrentWeapon?.QueueFree();
        CurrentWeapon = null;
        CurrentWeaponResource = null;
    }
}