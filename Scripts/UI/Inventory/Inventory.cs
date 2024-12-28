using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Inventory;

[Scene]
public partial class Inventory : Control
{
    [Node]
    private GridContainer slotsContainer;

    [Node]
    private ResourcePreloader resourcePreloader;

    [Node]
    private Button closeButton;

    [Node]
    private Button equipButton;

    private Player player => this.GetPlayer();
    private List<Slot> slots => slotsContainer.GetChildrenOfType<Slot>().ToList();
    private Slot selectedSlot => slots.FirstOrDefault(s => s.Selected);

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        equipButton.Toggled += OnEquipButtonToggle;
        closeButton.Pressed += () => GetTree().CreateTimer(0.1f).Timeout += Close;
        VisibilityChanged += OnVisibilityChanged;

        if (player != null)
            player.Inventory.ItemPickUp += OnItemPickup;
    }


    // TODO: Centralize ui opening and closing
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("menu"))
        {
            Close();
            return;
        }

        if (!@event.IsActionPressed("open_inventory")) return;

        Toggle();
    }

    private void OnItemPickup(Item _item)
    {
        var inventory = player.Inventory.Items;

        slots.ForEach(s => s.Clear());
        inventory.ForEach(i =>
        {
            var slot = slots.FirstOrDefault(slot => !slot.IsOccupied);

            if (slot == null)
            {
                var newSlot = resourcePreloader.InstanceSceneOrNull<Slot>();
                newSlot.Item = i;
                slotsContainer.AddChild(newSlot);
                return;
            }

            slot.Item = i;
        });
    }

    private void OnVisibilityChanged()
    {
        GetTree().Paused = Visible;
        if (!Visible) return;
        slots.First().Select();
    }

    private void OnEquipButtonToggle(bool toggled)
    {
        equipButton.Text = toggled ? "Unequip" : "Equip";
        equipButton.Modulate = toggled ? Colors.Red : Colors.White;
    }

    // TODO: Add inventory animation
    private void Toggle() => Visible = !Visible;
    private void Close() => Visible = false;
}