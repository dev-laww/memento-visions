using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
using Game.Resources;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;
using Type = Game.Resources.Type;

namespace Game.UI;

[Tool]
[Scene]
public partial class Inventory : Control
{
    [Node] private GridContainer slotsContainer;
    [Node] private ResourcePreloader resourcePreloader;

    [Node] private TextureButton closeButton;
    [Node] private Button equipButton;
    [Node] private Button materialItemsButton;
    [Node] private Button weaponItemsButton;

    [Node] private TextureRect selectedItemIcon;
    [Node] private Label selectedItemName;
    [Node] private Label selectedItemType;
    [Node] private Label selectedItemQuantity;
    [Node] private RichTextLabel selectedItemDescription;

    private Player Player;
    private List<Slot> slots => slotsContainer.GetChildrenOfType<Slot>().ToList();
    private Button selectedButton => weaponItemsButton.ButtonGroup.GetPressedButton() as Button;
    private ButtonGroup buttonGroup;
    private Item selectedItem;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        Player = this.GetPlayer();
        buttonGroup = weaponItemsButton.ButtonGroup;

        SetupEventHandlers();

        if (Player == null || Engine.IsEditorHint()) return;

        Reset();
    }

    private void SetupEventHandlers()
    {
        equipButton.Pressed += OnEquipButtonPress;
        buttonGroup.Pressed += OnButtonPressed;
        closeButton.Pressed += () => GetTree().CreateTimer(0.1f).Timeout += Close;
        VisibilityChanged += OnVisibilityChanged;
        slots.ForEach(slot => slot.Selected += OnSlotSelected);
        slots.First().Select();

        if (Player == null) return;

        Player.Inventory.ItemAdd += OnItemPickup;
    }

    private void Reset()
    {
        equipButton.Visible = false;
        materialItemsButton.ButtonPressed = true;
        FilterItems(Type.Material.ToString());
        Clear();
        slots.First().Select();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("menu"))
        {
            Close();
            return;
        }

        if (@event.IsActionPressed("open_inventory"))
            Toggle();
    }

    private void OnItemPickup(Item item)
    {
        var currentFilter = selectedButton?.Name.ToString().Replace("ItemsButton", "") ?? "Material";
        var allSlotsEmpty = slots.All(slot => slot.Item == null);

        // TODO: Fix item not showing up when all slots are empty
        if (item.Type.ToString() == currentFilter || allSlotsEmpty)
            FilterItems(currentFilter);
    }

    private void OnVisibilityChanged()
    {
        if (Engine.IsEditorHint() || Player == null) return;

        Player.SetProcessInput(!Visible);

        if (!Visible) return;

        Reset();
    }

    private void FilterItems(string filter)
    {
        if (Engine.IsEditorHint()) return;

        var type = (Type)Enum.Parse(typeof(Type), filter);

        Clear();
        var items = Player.Inventory.GetFilteredItems(type);

        foreach (var item in items)
        {
            var slot = slots.FirstOrDefault(slot => !slot.IsOccupied);
            if (slot == null)
            {
                var newSlot = resourcePreloader.InstanceSceneOrNull<Slot>();
                newSlot.Item = item;
                slotsContainer.AddChild(newSlot);
                continue;
            }

            slot.Item = item;
        }

        slots.First().Select();
    }

    private void OnEquipButtonPress()
    {
        if (selectedItem.UniqueName == Player.Inventory.CurrentWeapon?.UniqueName)
            Player.Inventory.ChangeWeapon(null);
        else
            Player.Inventory.ChangeWeapon(selectedItem.UniqueName);

        HandleEquipButton();
    }

    private void HandleEquipButton()
    {
        var currentWeapon = Player.Inventory.CurrentWeapon;
        var isCurrentWeapon = currentWeapon?.UniqueName == selectedItem?.UniqueName;
        var isWeapon = selectedItem?.Type == Type.Weapon;

        equipButton.Visible = isWeapon;

        if (!isWeapon) return;

        equipButton.ButtonPressed = isCurrentWeapon;
        equipButton.Text = isCurrentWeapon ? "Unequip" : "Equip";
        equipButton.Modulate = isCurrentWeapon ? Colors.Red : Colors.White;
    }

    private void Toggle() => Visible = !Visible;
    private void Close() => Visible = false;

    private void Clear()
    {
        slots.ForEach(slot => slot.Item = null);
        slots.First().Select();
    }

    private void OnSlotSelected(Slot slot)
    {
        var unselectedSlots = slots.Where(s => s != slot).ToList();

        unselectedSlots.ForEach(s => s.Deselect());

        NotifyPropertyListChanged();

        if (Engine.IsEditorHint()) return;

        var item = slot.Item;
        selectedItem = item;
        selectedItemIcon.Texture = item?.Icon;
        selectedItemName.Text = item?.Name;
        selectedItemType.Text = item?.Type.ToString();
        selectedItemQuantity.Text = item != null ? $"x{item.Value}" : null;
        selectedItemDescription.Text = item?.Description;

        HandleEquipButton();
    }

    private void OnButtonPressed(BaseButton button)
    {
        var filter = button.Name.ToString().Replace("ItemsButton", "");

        FilterItems(filter);
    }

    public override void _ExitTree()
    {
        equipButton.Pressed -= OnEquipButtonPress;
        VisibilityChanged -= OnVisibilityChanged;
        slots.ForEach(slot => slot.Selected -= OnSlotSelected);

        if (Player == null) return;

        Player.Inventory.ItemAdd -= OnItemPickup;
    }
}