using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;
using Type = Game.Resources.Type;

namespace Game.UI.Inventory;

// TODO: Fix bugs

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

    [Node]
    private Button materialItemsButton;

    [Node]
    private Button weaponItemsButton;

    [Node]
    private TextureRect selectedItemIcon;

    [Node]
    private Label selectedItemName;

    [Node]
    private Label selectedItemType;

    [Node]
    private Label selectedItemQuantity;

    [Node]
    private RichTextLabel selectedItemDescription;

    private Player player => this.GetPlayer();
    private List<Slot> slots => slotsContainer.GetChildrenOfType<Slot>().ToList();
    private Button selectedButton => weaponItemsButton.ButtonGroup.GetPressedButton() as Button;
    private Item selectedItem;
    private Type selectedType;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        weaponItemsButton.ButtonGroup.Pressed += button => OnButtonGroupPressed(button as Button);
        equipButton.Toggled += OnEquipButtonToggle;
        closeButton.Pressed += () => GetTree().CreateTimer(0.1f).Timeout += Close;
        VisibilityChanged += OnVisibilityChanged;

        if (player == null) return;

        FilterItems(Type.Material.ToString());
        Clear();

        player.Inventory.ItemPickUp += OnItemPickup;
        selectedItemIcon.Texture = null;
        selectedItemName.Text = "";
        selectedItemType.Text = "";
        selectedItemQuantity.Text = "";
        selectedItemDescription.Text = "";
        equipButton.Visible = false;
    }

    public override void _Process(double delta)
    {
        var item = slots.FirstOrDefault(s => s.Selected)?.Item;

        equipButton.ButtonPressed = player.Inventory.CurrentWeapon.UniqueName == item?.UniqueName;

        if (item == null || selectedItem == item) return;

        selectedItemIcon.Texture = item.Icon;
        selectedItemName.Text = item.Name;
        selectedItemType.Text = item.Type.ToString();
        selectedItemQuantity.Text = $"x{item.Value.ToString()}";
        selectedItemDescription.Text = item.Description;

        selectedItem = item;
        equipButton.Visible = item.Type == Type.Weapon || weaponItemsButton.ButtonPressed;

        var pressed = equipButton.ButtonPressed;
        equipButton.Text = pressed ? "Unequip" : "Equip";
        equipButton.Modulate = pressed ? Colors.Red : Colors.White;
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

    private void OnItemPickup(Item item)
    {
        var filter = selectedButton?.Name.ToString().Replace("ItemsButton", "") ?? "Material";

        if (item.Type.ToString() != filter) return;

        FilterItems(filter);
    }

    private void OnVisibilityChanged()
    {
        GetTree().Paused = Visible;

        if (!Visible) return;

        slots.First().Select();
        materialItemsButton.ButtonPressed = true;
        FilterItems(Type.Material.ToString());
    }

    private void OnEquipButtonToggle(bool toggled)
    {
        var currentWeapon = player.Inventory.CurrentWeapon;

        if (selectedItem == null || currentWeapon.UniqueName == selectedItem.UniqueName) return;

        equipButton.Text = toggled ? "Unequip" : "Equip";
        equipButton.Modulate = toggled ? Colors.Red : Colors.White;
        player.Inventory.ChangeWeapon(selectedItem.UniqueName);
    }

    private void OnButtonGroupPressed(Button button)
    {
        if (player == null) return;

        var filter = button.Name.ToString().Replace("ItemsButton", "");

        FilterItems(filter);
    }

    private void FilterItems(string filter)
    {
        var type = (Type)Enum.Parse(typeof(Type), filter);

        var hasOccupiedSlot = slots.Any(slot => slot.IsOccupied);

        if (selectedType == type && hasOccupiedSlot) return;

        Clear();

        var items = player.Inventory.GetFilteredItems(type);

        items.ForEach(item =>
        {
            var slot = slots.FirstOrDefault(slot => !slot.IsOccupied);

            if (slot == null)
            {
                var newSlot = resourcePreloader.InstanceSceneOrNull<Slot>();
                newSlot.Item = item;
                slotsContainer.AddChild(newSlot);
                return;
            }

            slot.Item = item;
        });

        selectedType = type;
    }

    private void Clear() => slots.ForEach(slot => slot.Item = null);

    // TODO: Add inventory animation
    private void Toggle() => Visible = !Visible;
    private void Close() => Visible = false;
}