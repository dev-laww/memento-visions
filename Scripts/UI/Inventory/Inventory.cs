using System.Linq;
using Game.Entities.Player;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Inventory : Control
{
    [Node]
    private GridContainer slotsContainer;

    [Node]
    private ResourcePreloader resourcePreloader;

    [Node]
    private Button closeButton;

    private Player player => this.GetPlayer();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        player.ItemPickedUp += OnItemPickup;
        closeButton.Pressed += Close;
        VisibilityChanged += () => GetTree().Paused = Visible;
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
        var inventory = player.Inventory;
        var slots = slotsContainer.GetChildrenOfType<Slot>().ToList();

        slots.ForEach(s => s.Item = null);
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

    // TODO: Add inventory animation
    private void Toggle() => Visible = !Visible;
    private void Close() => Visible = false;
}