using System.Collections.Generic;
using Godot;
using ItemResource = Game.Resources.Item;
using WeaponResource = Game.Resources.Weapon;

namespace Game.Components.Managers;

[GlobalClass]
public partial class InventoryManager : Node
{
    [Signal]
    public delegate void ItemPickUpEventHandler(ItemResource item);

    [Signal]
    public delegate void ItemConsumeEventHandler(ItemResource item);

    [Signal]
    public delegate void WeaponChangedEventHandler(WeaponResource weapon);

    public readonly List<ItemResource> Items = new();

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
}