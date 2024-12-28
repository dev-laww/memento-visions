using System.Collections.Generic;
using System.Linq;
using Game.Resources;
using Godot;
using ItemResource = Game.Resources.Item;
using WeaponResource = Game.Resources.Weapon;

namespace Game.Components.Managers;

[GlobalClass]
public partial class InventoryManager : Node
{
    [Export]
    private WeaponManager weaponManager;

    [Signal]
    public delegate void ItemPickUpEventHandler(ItemResource item);

    [Signal]
    public delegate void ItemConsumeEventHandler(ItemResource item);

    private readonly List<ItemResource> Items = new();

    public WeaponResource CurrentWeapon => weaponManager.CurrentWeapon.Resource;

    public void ChangeWeapon(string uniqueName) => weaponManager.ChangeWeapon(uniqueName);

    public void PickUpItem(ItemResource item)
    {
        var existing = Items.Find(i => i.UniqueName == item.UniqueName);

        if (existing is { Stackable: true })
            Items[Items.IndexOf(existing)] += item;
        else
            Items.Add(item);

        EmitSignal(SignalName.ItemPickUp, item);
    }

    public void ConsumeItem(ItemResource item)
    {
        var existing = Items.Find(i => i.UniqueName == item.UniqueName);

        if (existing is null)
            return;

        if (existing.Stackable)
        {
            existing -= item;

            if (existing.Value == 0)
                Items.Remove(existing);
        }
        else
        {
            Items.Remove(existing);
        }
    }

    public List<ItemResource> GetFilteredItems(Type type) => Items.Where(i => i.Type == type).ToList();
}