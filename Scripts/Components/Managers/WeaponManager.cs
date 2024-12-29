using System.Collections.Generic;
using Game.Components.Battle;
using Game.Utils.JSON;
using Godot;
using Item = Game.Utils.JSON.Models.Item;
using Variant = Game.Resources.Variant;

namespace Game.Components.Managers;

// TODO: implement weapon unlocking system
[GlobalClass]
public partial class WeaponManager : Node2D
{
    [Signal] public delegate void WeaponChangedEventHandler();

    public Weapon CurrentWeapon { get; private set; }
    public Variant CurrentWeaponType => CurrentWeapon?.Resource.Variant ?? Variant.Dagger;

    private const string WEAPONS_DATA_PATH = "res://data/weapons.json";
    private readonly List<Item> weaponsData = JSON.Load<List<Item>>(WEAPONS_DATA_PATH);

    // public override void _Ready() => ChangeWeapon("weapon:gun");

    public void ChangeWeapon(string weapon)
    {
        CurrentWeapon?.QueueFree();

        if (weapon == null)
        {
            CurrentWeapon = null;
            EmitSignal(SignalName.WeaponChanged);
            return;
        }

        var data = weaponsData.Find(w => w.UniqueName == weapon);

        if (data == null)
        {
            GD.PushWarning("Weapon not found");
            return;
        }

        var scene = GD.Load<PackedScene>(data.Scene);

        var instance = scene.Instantiate<Weapon>();

        instance.SetVisible(false);
        CurrentWeapon = instance;
        AddChild(CurrentWeapon);
        EmitSignal(SignalName.WeaponChanged);
    }

    public void RemoveWeapon()
    {
        EmitSignal(SignalName.WeaponChanged);

        CurrentWeapon?.QueueFree();
        CurrentWeapon = null;
    }

    public SignalAwaiter Animate(string direction) => CurrentWeapon.Animate(direction);

    public void Reload()
    {
        weaponsData.Clear();

        var json = JSON.Load<List<Item>>(WEAPONS_DATA_PATH);
        json.ForEach(w => weaponsData.Add(w));
    }
}