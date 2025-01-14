using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
// using Game.Resources;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;
// using Type = Game.Resources.Type;

namespace Game.UI.Overlays;

// I am here during new year's eve, 2025. Programming is fun. Last commit of 2024. Happy new year!
// Doing thesis sucks :DDD 
// - 31/12/2024 - Lawrence

// [Tool]
[Scene]
public partial class Inventory : Overlay
{
    // UI Components
    [Node] private GridContainer slotsContainer;
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private TextureButton closeButton;
    [Node] private Button equipButton;
    [Node] private Button materialItemsButton;
    [Node] private Button weaponItemsButton;

    // Selected Item Display
    [Node] private TextureRect selectedItemIcon;
    [Node] private Label selectedItemName;
    [Node] private Label selectedItemType;
    [Node] private Label selectedItemQuantity;
    [Node] private RichTextLabel selectedItemDescription;

    // State
    private Player player;
    private ButtonGroup buttonGroup;
    // private Item selectedItem;

    // Properties
    private List<Slot> Slots => slotsContainer.GetChildrenOfType<Slot>().ToList();
    private Button SelectedFilterButton => buttonGroup?.GetPressedButton() as Button;
    private string CurrentFilter => SelectedFilterButton?.Name.ToString().Replace("ItemsButton", "") ?? "Material";

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        InitializeInventory();

        if (!ShouldInitializeGameplay()) return;

        Reset();
    }

    public override void _ExitTree()
    {
        // cleanup events
        Slots.ForEach(slot => slot.Selected -= OnSlotSelected);
        //
        // if (player != null)
        //     player.Inventory.ItemAdd -= OnItemPickup;
    }

    private void InitializeInventory()
    {
        player = this.GetPlayer();
        buttonGroup = weaponItemsButton.ButtonGroup;
        SetupEventHandlers();
    }

    private bool ShouldInitializeGameplay() => player != null && !Engine.IsEditorHint();

    private void SetupEventHandlers()
    {
        // UI Events
        equipButton.Pressed += OnEquipButtonPress;
        buttonGroup.Pressed += OnFilterButtonPressed;
        closeButton.Pressed += HandleCloseButtonPress;
        VisibilityChanged += OnVisibilityChanged;

        // Slot Events
        InitializeSlotEvents();

        // Player Events
        // if (player != null)
        //     player.Inventory.ItemAdd += OnItemPickup;
    }

    private void InitializeSlotEvents()
    {
        Slots.ForEach(slot => slot.Selected += OnSlotSelected);
        Slots.First().Select();
    }

    private void HandleCloseButtonPress() => GetTree().CreateTimer(0.1f).Timeout += Close;

    private void Reset()
    {
        // equipButton.Visible = false;
        // materialItemsButton.ButtonPressed = true;
        // ClearSlots();
        // FilterItems(Type.Material.ToString());
    }

    // private void OnItemPickup(Item item)
    // {
    //     var shouldRefreshDisplay = ShouldRefreshDisplayOnPickup(item, CurrentFilter);
    //
    //     if (shouldRefreshDisplay)
    //         FilterItems(CurrentFilter);
    // }

    // private bool ShouldRefreshDisplayOnPickup(Item item, string currentFilter)
    // {
    //     return item.Type.ToString() == currentFilter || Slots.All(slot => slot.Item == null);
    // }

    private void OnVisibilityChanged()
    {
        if (!Engine.IsEditorHint() && Visible && player != null)
            Reset();
    }

    private void FilterItems(string filter)
    {
        if (Engine.IsEditorHint()) return;

        var type = (Type)Enum.Parse(typeof(Type), filter);
        ClearSlots();
        DisplayFilteredItems(type);
    }

    private void DisplayFilteredItems(Type type)
    {
        // var items = player.Inventory.GetFilteredItems(type);
        //
        // foreach (var item in items)
        // {
        //     var slot = FindOrCreateSlot();
        //
        //     if (slot == null) continue;
        //
        //     if (item.Stackable)
        //     {
        //         slot.Item = item;
        //         continue;
        //     }
        //
        //     for (var i = 0; i < item.Value; i++)
        //     {
        //         var newSlot = FindOrCreateSlot();
        //         var duplicate = item.Duplicate();
        //         duplicate.Value = 1;
        //         newSlot.Item = duplicate;
        //     }
        // }

        Slots.First().Select();
    }

    private Slot FindOrCreateSlot()
    {
        // var existingSlot = Slots.FirstOrDefault(slot => !slot.IsOccupied);
        // if (existingSlot != null) return existingSlot;
        //
        var newSlot = resourcePreloader.InstanceSceneOrNull<Slot>();
        slotsContainer.AddChild(newSlot);
        return newSlot;
    }

    private void OnEquipButtonPress()
    {
        // var isCurrentWeapon = selectedItem.UniqueName == player.Inventory.CurrentWeapon?.UniqueName;
        //
        // player.Inventory.ChangeWeapon(isCurrentWeapon ? null : selectedItem.UniqueName);

        UpdateEquipButtonState();
    }

    private void UpdateEquipButtonState()
    {
        // var currentWeapon = player.Inventory.CurrentWeapon;
        // var isCurrentWeapon = currentWeapon?.UniqueName == selectedItem?.UniqueName;
        // var isWeapon = selectedItem?.Type == Type.Weapon;
        //
        // equipButton.Visible = isWeapon;
        //
        // if (!isWeapon) return;
        //
        // equipButton.ButtonPressed = isCurrentWeapon;
        // equipButton.Text = isCurrentWeapon ? "Unequip" : "Equip";
        // equipButton.Modulate = isCurrentWeapon ? Colors.Red : Colors.White;
    }

    private void ClearSlots()
    {
        Slots.ForEach(slot => slot.Clear());
        Slots.First().Select();
    }

    private void OnSlotSelected(Slot selectedSlot)
    {
        DeselectOtherSlots(selectedSlot);

        if (Engine.IsEditorHint() || player == null) return;

        // selectedItem = selectedSlot.Item;
        // selectedItemIcon.Texture = selectedItem?.Icon;
        // selectedItemName.Text = selectedItem?.Name;
        // selectedItemType.Text = selectedItem?.Type.ToString();
        // selectedItemQuantity.Text = selectedItem != null ? $"x{selectedItem.Value}" : null;
        // selectedItemDescription.Text = selectedItem?.Description;

        UpdateEquipButtonState();
    }

    private void DeselectOtherSlots(Slot selectedSlot)
    {
        var unselectedSlots = Slots.Where(s => s != selectedSlot);

        foreach (var slot in unselectedSlots)
            slot.Deselect();

        NotifyPropertyListChanged();
    }

    private void OnFilterButtonPressed(BaseButton button)
    {
        if (Engine.IsEditorHint() || player == null) return;

        var filter = button.Name.ToString().Replace("ItemsButton", "");
        FilterItems(filter);
    }
}