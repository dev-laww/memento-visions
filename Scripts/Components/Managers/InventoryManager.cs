using System.Collections.Generic;
using System.Linq;
using Game.Resources;
using Godot;

namespace Game.Components.Managers;

[GlobalClass]
public partial class InventoryManager : Node
{
    [Export] private WeaponManager weaponManager;

    [Signal] public delegate void ItemAddEventHandler(Item item);
    [Signal] public delegate void ItemRemoveEventHandler(Item item);

    public readonly List<Item> Items = new();

    public Weapon CurrentWeapon => weaponManager.CurrentWeapon?.Resource;

    public void ChangeWeapon(string uniqueName) => weaponManager.ChangeWeapon(uniqueName);

    public void AddItem(Item item)
    {
        var existing = Items.Find(i => i.UniqueName == item.UniqueName);

        if (existing is { Stackable: true })
            Items[Items.IndexOf(existing)] += item;
        else
            Items.Add(item);

        EmitSignal(SignalName.ItemAdd, item);
    }

    // TODO: clean this up
    public void RemoveItem(Item item)
    {
        var existing = Items.Find(i => i.UniqueName == item.UniqueName);

        if (existing is null)
            return;

        if (existing.Stackable)
        {
            var remove = existing.Value <= item.Value;

            if (remove)
                Items.Remove(existing);
            else
                existing -= item;
        }
        else
            Items.Remove(existing);


        EmitSignal(SignalName.ItemRemove, item);
    }

    public List<Item> GetItemsByUniqueName(string uniqueName)
    {
        return Items.Where(item => item.UniqueName == uniqueName).ToList();
    }

    public List<Item> GetFilteredItems(Type type) => Items.Where(i => i.Type == type).ToList();
}