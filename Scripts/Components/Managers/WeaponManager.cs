using System.Linq;
using Game.Common;
using Game.Common.Utilities;
using Game.Entities;
using Game.Autoload;
using Game.Data;
using Godot;
using System.CommandLine.IO;

namespace Game.Components;

[GlobalClass, Icon("res://assets/icons/weapon-manager.svg")]
public partial class WeaponManager : Node
{
    public WeaponComponent WeaponComponent { get; private set; }
    public Item Weapon { get; private set; }
    public bool CanAttack => Weapon != null && WeaponComponent != null;
    private Entity parent;
    public SignalAwaiter AnimationFinished => WeaponComponent.AnimationFinished;

    public override void _Ready()
    {
        parent = GetParent<Entity>();

        if (SaveManager.Data.Player.Equipped == string.Empty) return;

        var weapon = ItemRegistry.Get(SaveManager.Data.Player.Equipped);

        if (weapon == null) return;

        Equip(weapon);
    }

    public override void _EnterTree()
    {
        CommandInterpreter.Register(this);
    }


    public override void _ExitTree()
    {
        CommandInterpreter.Unregister(this);

        if (Weapon == null) return;

        SaveManager.Data.Player.Equipped = Weapon.Id;
        SaveManager.Save();
    }

    public void Equip(Item weapon)
    {
        if (weapon == null || weapon.Id == Weapon?.Id || weapon.ItemCategory != Item.Category.Weapon) return;

        WeaponComponent?.QueueFree();

        if (Weapon != null)
            parent.StatsManager.DecreaseDamage(Weapon.DamagePercentBuff, StatsManager.ModifyMode.Percentage);

        Weapon = weapon;
        WeaponComponent = weapon.Component.Instantiate<WeaponComponent>();
        WeaponComponent.Visible = false;

        SaveManager.Data.Player.Equipped = Weapon.Id;

        parent.StatsManager.IncreaseDamage(weapon.DamagePercentBuff, StatsManager.ModifyMode.Percentage);
        parent.CallDeferred("add_child", WeaponComponent);
        Log.Debug($"Equipped {weapon}");
    }

    public void Unequip()
    {
        parent.StatsManager.DecreaseDamage(Weapon.DamagePercentBuff, StatsManager.ModifyMode.Percentage);

        WeaponComponent?.QueueFree();
        Weapon = null;
        WeaponComponent = null;

        SaveManager.Data.Player.Equipped = string.Empty;

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

    [Command(Name = "equip", Description = "Equips a weapon")]
    public void EquipCommand(string id)
    {
        var weapon = ItemRegistry.Get(id);

        if (weapon == null)
        {
            DeveloperConsole.Console.Error.WriteLine($"Item {id} not found.");
            return;
        }

        Equip(weapon);
    }

    [Command(Name = "unequip", Description = "Unequips the weapon")]
    public void UnequipCommand() => Unequip();
}