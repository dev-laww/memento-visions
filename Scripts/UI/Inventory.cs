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

    [Node] private Button closeButton;
    [Node] private Button equipButton;
    [Node] private Button materialItemsButton;
    [Node] private Button weaponItemsButton;

    [Node] private TextureRect selectedItemIcon;
    [Node] private Label selectedItemName;
    [Node] private Label selectedItemType;
    [Node] private Label selectedItemQuantity;
    [Node] private RichTextLabel selectedItemDescription;

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
        SetupEventHandlers();

        if (player == null || Engine.IsEditorHint()) return;

        Reset();
    }

    private void SetupEventHandlers()
    {
        equipButton.Toggled += OnEquipButtonToggle;
        closeButton.Pressed += () => GetTree().CreateTimer(0.1f).Timeout += Close;
        VisibilityChanged += OnVisibilityChanged;
        slots.ForEach(slot => slot.Selected += OnSlotSelected);
        slots.First().Select();

        if (player == null) return;

        player.Inventory.ItemAdd += OnItemPickup;
    }

    private void Reset()
    {
        equipButton.Visible = false;
        materialItemsButton.ButtonPressed = true;
        FilterItems(Type.Material.ToString());
        Clear();
        SelectItem(null);
        slots.First().Select();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || player == null) return;

        var selectedSlotItem = slots.FirstOrDefault(s => s.IsSelected)?.Item;

        var filter = selectedButton.Name.ToString().Replace("ItemsButton", "");

        if (selectedType.ToString() != filter)
            FilterItems(filter);

        if (selectedSlotItem == null || selectedSlotItem == selectedItem) return;

        SelectItem(selectedSlotItem);
        HandleEquipButton();
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

        if (item.Type.ToString() == currentFilter || allSlotsEmpty)
            FilterItems(currentFilter);
    }

    private void OnVisibilityChanged()
    {
        if (Engine.IsEditorHint() || player == null) return;

        player.SetProcessInput(!Visible);

        if (!Visible) return;

        Reset();
    }

    private void FilterItems(string filter)
    {
        if (Engine.IsEditorHint()) return;

        var type = (Type)Enum.Parse(typeof(Type), filter);

        Clear();
        var items = player.Inventory.GetFilteredItems(type);

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

        selectedType = type;
    }

    private void SelectItem(Item item)
    {
        selectedItem = item;
        UpdateItemDisplay(item);
    }

    private void UpdateItemDisplay(Item item)
    {
        selectedItemIcon.Texture = item?.Icon;
        selectedItemName.Text = item?.Name;
        selectedItemType.Text = item?.Type.ToString();
        selectedItemQuantity.Text = item != null ? $"x{item.Value}" : null;
        selectedItemDescription.Text = item?.Description;
    }

    private void OnEquipButtonToggle(bool toggled)
    {
        if (!toggled)
            player.Inventory.ChangeWeapon(null);
        else
            player.Inventory.ChangeWeapon(selectedItem.UniqueName);

        HandleEquipButton();
    }

    private void HandleEquipButton()
    {
        if (selectedItem == null) return;

        var currentWeapon = player.Inventory.CurrentWeapon;
        var isCurrentWeapon = currentWeapon?.UniqueName == selectedItem.UniqueName;

        equipButton.ButtonPressed = isCurrentWeapon;
        equipButton.Visible = selectedItem.Type == Type.Weapon;
        equipButton.Text = isCurrentWeapon ? "Unequip" : "Equip";
        equipButton.Modulate = isCurrentWeapon ? Colors.Red : Colors.White;
    }

    private void Clear() => slots.ForEach(slot => slot.Item = null);
    private void Toggle() => Visible = !Visible;
    private void Close() => Visible = false;

    private void OnSlotSelected(Slot slot)
    {
        var unselectedSlots = slots.Where(s => s != slot).ToList();

        unselectedSlots.ForEach(s => s.Deselect());

        NotifyPropertyListChanged();
    }

    public override void _ExitTree()
    {
        slots.ForEach(slot => slot.Selected -= OnSlotSelected);

        if (player == null) return;

        player.Inventory.ItemAdd -= OnItemPickup;
    }
}