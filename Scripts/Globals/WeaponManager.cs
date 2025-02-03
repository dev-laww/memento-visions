using Game.Common;
using Game.Components.Battle;
using Game.Entities.Characters;
using Game.Resources;
using Game.Utils;
using Game.Utils.Extensions;
using Godot;

namespace Game.Globals;

// TODO: Loading weapon from save file and if player is freed, re-equip weapon
public partial class WeaponManager : Global<WeaponManager>
{
    public static bool CanAttack => CurrentWeapon != null;
    public static WeaponComponent CurrentWeapon { get; private set; }
    public static SignalAwaiter AnimationFinished => CurrentWeapon?.AnimationFinished ?? default;
    public static Item CurrentWeaponResource { get; private set; }

    public static void Equip(Item weapon)
    {
        var player = Instance.GetPlayer();

        if (player == null || weapon.Component.ResourcePath == CurrentWeapon?.SceneFilePath) return;

        var component = weapon.Component.Instantiate<WeaponComponent>();

        CurrentWeapon?.QueueFree();
        player.AddChild(component);
        CurrentWeapon = component;
        CurrentWeaponResource = weapon;
        Log.Debug($"{weapon} equipped.");
    }

    public static void Attack(string direction)
    {
        CurrentWeapon?.Animate(direction);
    }

    public static void Unequip()
    {
        Log.Debug($"{CurrentWeaponResource} unequipped.");
        CurrentWeapon?.QueueFree();
        CurrentWeapon = null;
        CurrentWeaponResource = null;
    }
}