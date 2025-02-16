using System;
using System.Collections.Generic;
using System.Linq;
using Game.Globals;
using Game.Data;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

// I am here during new year's eve, 2025. Programming is fun. Last commit of 2024. Happy new year!
// Doing thesis sucks :DDD 
// - 31/12/2024 - Lawrence

// [Tool]
[Scene]
public partial class Inventory : Overlay
{
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private TextureButton closeButton;
    [Node] private GridContainer slotsContainer;
    [Node] private Button materialButton;
    [Node] private TextureRect selectedItemIcon;
    [Node] private Label selectedItemName;
    [Node] private Label selectedItemCategory;
    [Node] private Label selectedItemQuantity;
    [Node] private RichTextLabel selectedItemDescription;
    [Node] private Button selectedItemActionButton;

    private List<Slot> slots;
    private Item.Category currentCategory = Item.Category.Material;
    private Item SelectedItem => slots.FirstOrDefault(s => s.Selected)?.Item?.Item;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        var player = this.GetPlayer();
        if (player is null) return;

        slots = slotsContainer.GetChildrenOfType<Slot>().ToList();

        slots.ForEach(slot => slot.Pressed += SelectSlot);
        closeButton.Pressed += Close;
        selectedItemActionButton.Toggled += OnButtonToggle;
        materialButton.ButtonGroup.Pressed += OnItemCategoryPress;
        player.InventoryManager.Updated += OnInventoryUpdate;

        PopulateSlots(currentCategory);
    }

    private void SelectSlot(Slot slot)
    {
        var selectedSlot = slots.FirstOrDefault(s => s.Selected);

        if (selectedSlot is null)
        {
            slot.Selected = true;
            return;
        }

        if (selectedSlot == slot) return;

        slot.Selected = true;
        selectedSlot.Selected = false;
        UpdateSelectedItem(slot.Item);
    }

    private void UpdateSelectedItem(ItemGroup item)
    {
        selectedItemIcon.Texture = item?.Item.Icon;
        selectedItemName.Text = item?.Item.Name;
        selectedItemCategory.Text = item?.Item.ItemCategory.ToString();
        selectedItemQuantity.Text = item is null ? string.Empty : item.Quantity > 999 ? "x999+" : $"x{item.Quantity}";
        selectedItemDescription.Text = item?.Item.Description;

        selectedItemActionButton.Visible = item?.Item.ItemCategory is Item.Category.Weapon or Item.Category.Consumable;
        selectedItemActionButton.ToggleMode = item?.Item.ItemCategory == Item.Category.Weapon;
        selectedItemActionButton.ButtonPressed = item?.Item.Id == this.GetPlayer()?.WeaponManager.Weapon?.Id;
        selectedItemActionButton.Text = item?.Item.ItemCategory switch
        {
            Item.Category.Weapon => selectedItemActionButton.ButtonPressed ? "Unequip" : "Equip",
            Item.Category.Consumable => "Use",
            _ => string.Empty
        };
    }

    private void OnItemCategoryPress(BaseButton button)
    {
        var meta = button.GetMeta("Category").AsString();
        var category = Enum.Parse<Item.Category>(meta);

        if (currentCategory == category) return;

        currentCategory = category;

        PopulateSlots(category);
    }

    private void PopulateSlots(Item.Category category)
    {
        var player = this.GetPlayer();

        if (player is null) return;

        var items = player.InventoryManager.GetItemsFromCategory(category);

        slots.Where(slot => slot.Item != null).ToList().ForEach(slot => slot.Item = null);

        for (var i = 0; i < items.Count; i++)
        {
            slots[i].Item = items[i];
        }

        var slot = slots.First();
        SelectSlot(slot);
        UpdateSelectedItem(slot.Item);
    }

    private void OnInventoryUpdate(ItemGroup group)
    {
        var (item, _) = group;

        if (item.ItemCategory != currentCategory) return;

        var slot = slots.FirstOrDefault(s => s.Item?.Item.Id == item.Id) ??
                   slots.First(s => s.Item is null);

        slot.Item = group.Quantity > 0 ? group : null;

        if (slot.Selected)
        {
            UpdateSelectedItem(slot.Item);
        }
    }

    private void OnButtonToggle(bool pressed)
    {
        var player = this.GetPlayer();

        if (SelectedItem is null || SelectedItem.ItemCategory != Item.Category.Weapon || player is null) return;

        if (pressed)
        {
            player.WeaponManager.Equip(SelectedItem);
            selectedItemActionButton.Text = "Unequip";
        }
        else if (SelectedItem.Id == player.WeaponManager.Weapon?.Id)
        {
            player.WeaponManager.Unequip();
            selectedItemActionButton.Text = "Equip";
        }
    }

    public override void Close()
    {
        base.Close();

        materialButton.ButtonPressed = true;
    }
}