using System.Linq;
using Game.Common;
using Game.Components.Battle;
using Game.Resources;
using Godot;

namespace Game.Components.Managers;

[Tool]
[GlobalClass]
public partial class WeaponManager : Node
{
    public WeaponComponent WeaponComponent { get; private set; }
    public Item Weapon { get; private set; }
    public bool CanAttack => Weapon != null && WeaponComponent != null;
    public SignalAwaiter AnimationFinished => WeaponComponent.AnimationFinished;

    public void Equip(Item weapon)
    {
        if (weapon == null || weapon.Id == Weapon?.Id) return;

        WeaponComponent?.QueueFree();

        Weapon = weapon;
        WeaponComponent = weapon.Component.Instantiate<WeaponComponent>();

        Owner.AddChild(WeaponComponent);
        Log.Debug($"Equipped {weapon}");
    }

    public void Unequip()
    {
        WeaponComponent?.QueueFree();
        Weapon = null;
        Weapon = null;
        Log.Debug($"Unequipped {Weapon}");
    }

    public void Animate()
    {
        if (!Owner.GetChildren().OfType<WeaponComponent>().Any())
        {
            WeaponComponent = Weapon.Component.Instantiate<WeaponComponent>();
            Owner.AddChild(WeaponComponent);
        }

        WeaponComponent?.Animate();
    }
}