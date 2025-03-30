using Game.Common;
using Game.Common.Utilities;
using Game.Entities;
using Game.Autoload;
using Game.Data;
using Godot;
using System.CommandLine.IO;
using Game.Utils.Extensions;
using Game.Utils;

namespace Game.Components;

[GlobalClass, Icon("res://assets/icons/weapon-manager.svg")]
public partial class WeaponManager : Node
{
    public WeaponComponent WeaponComponent { get; private set; }
    public Item Weapon { get; private set; }
    public bool CanAttack => Weapon != null && WeaponComponent != null;
    private Player player;

    public bool IsUsingDagger => Weapon?.WeaponType == Item.Type.Dagger;
    public bool IsUsingSword => Weapon?.WeaponType == Item.Type.Sword;
    public bool IsUsingWhip => Weapon?.WeaponType == Item.Type.Whip;

    private Vector2 blendPosition;

    public override void _Ready()
    {
        player = GetParent<Player>();

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
            player.StatsManager.DecreaseDamage(Weapon.DamagePercentBuff, StatsManager.ModifyMode.Percentage);

        Weapon = weapon;
        WeaponComponent = Weapon.Component.InstantiateOrNull<WeaponComponent>();

        if (WeaponComponent == null)
        {
            Log.Error($"Failed to instantiate weapon component for {weapon}");
            return;
        }


        SaveManager.Data.Player.Equipped = Weapon.Id;

        player.StatsManager.IncreaseDamage(weapon.DamagePercentBuff, StatsManager.ModifyMode.Percentage);
        this.GetPlayer()?.Center.CallDeferred("add_child", WeaponComponent);

        Log.Debug($"Equipped {weapon}");
    }

    public void Unequip()
    {
        player.StatsManager.DecreaseDamage(Weapon.DamagePercentBuff, StatsManager.ModifyMode.Percentage);

        WeaponComponent?.QueueFree();
        Weapon = null;
        WeaponComponent = null;

        SaveManager.Data.Player.Equipped = string.Empty;

        Log.Debug($"Unequipped {Weapon}");
    }

    public void Animate(int combo)
    {
        WeaponComponent?.Animate(combo);

        if (WeaponComponent == null) return;

        var shape = Weapon.WeaponType switch
        {
            Item.Type.Dagger => new RectangleShape2D { Size = new Vector2(25, 40) },
            Item.Type.Sword => new RectangleShape2D { Size = new Vector2(40, 40) },
            Item.Type.Whip => new RectangleShape2D { Size = new Vector2(80, 25) },
            _ => null
        }; // TODO: balance the hitboxes

        var difference = player.InputManager.GetGlobalMousePosition() - player.Center.GlobalPosition;
        var angle = difference.Angle();

        new DamageFactory.HitBoxBuilder(player.Center.GlobalPosition)
            .SetShape(shape)
            .SetRotation(angle)
            .SetDelay(Weapon.WeaponType == Item.Type.Whip ? 0.2f : 0)
            .SetShapeOffset(new Vector2(shape.Size.X / 2, 0))
            .SetOwner(player)
            .SetDamage(player.StatsManager.Damage)
            .SetDuration(0.1f)
            .Build();
    }

    public void SetBlendPosition(Vector2 position)
    {
        blendPosition = position;
        WeaponComponent?.SetBlendPosition(position);
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