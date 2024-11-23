using System.Collections.Generic;
using Game.Resources;
using Game.Utils.JSON;
using Godot;
using Item = Game.Utils.JSON.Models.Item;

namespace Game.Components.Managers;

// TODO: implement weapon unlocking system
public partial class WeaponManager : Node2D
{
    [Signal]
    public delegate void WeaponChangedEventHandler();

    [Export]
    public WeaponData CurrentWeaponData { get; private set; }

    private const string WEAPONS_DATA_PATH = "res://data/weapons.json";
    private readonly List<Item> weaponsData = JSON.Load<List<Item>>(WEAPONS_DATA_PATH);
    private readonly List<WeaponData> weapons = new();

    public override void _Ready()
    {
        weaponsData.ForEach(w => weapons.Add(GD.Load<WeaponData>(w.Resource)));
        CurrentWeaponData = weapons.Find(w => w.UniqueName == "weapon:dagger"); // default weapon or none depending on the story
    }

    public void ChangeWeapon(string weapon)
    {
        EmitSignal(SignalName.WeaponChanged);

        CurrentWeaponData = weapons.Find(w => w.UniqueName == weapon);
    }

    public void RemoveWeapon()
    {
        EmitSignal(SignalName.WeaponChanged);

        CurrentWeaponData = null;
    }

    public void AddWeapon(string weapon)
    {
        var data = weaponsData.Find(w => w.UniqueName == weapon);

        if (data == null) return;

        var resource = GD.Load<WeaponData>(data.Resource);

        weapons.Add(resource);
    }

    public void ReloadWeapons()
    {
        weapons.Clear();
        weaponsData.Clear();

        var json = JSON.Load<List<Item>>(WEAPONS_DATA_PATH);

        json.ForEach(w => weaponsData.Add(w));
        weaponsData.ForEach(w => weapons.Add(GD.Load<WeaponData>(w.Resource)));
    }
}