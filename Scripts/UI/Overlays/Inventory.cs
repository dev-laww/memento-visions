using System;
using System.Collections.Generic;
using System.Linq;
using Game.Autoload;
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
    [Node] private Button selectedItemQuickUseButton;
    [Node] private AudioStreamPlayer2D sfxClose;
    [Node] private AudioStreamPlayer2D sfxOpen;
    [Node] private AudioStreamPlayer2D sfxClick;

    private List<Slot> slots;
    private Item.Category currentCategory = Item.Category.Material;
    private Item selectedItem => slots.FirstOrDefault(s => s.Selected)?.Item?.Item;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    // TODO: Refactor this to rewire when player dies and respawns
    public override void _Ready()
    {
        var player = this.GetPlayer();
        if (player is null) return;

        slots = [.. slotsContainer.GetChildrenOfType<Slot>()];

        slots.ForEach(slot => slot.Pressed += SelectSlot);
        closeButton.Pressed += Close;
        selectedItemActionButton.Toggled += OnActionButtonToggle;
        selectedItemActionButton.Pressed += OnActionButtonPress;
        selectedItemQuickUseButton.Toggled += OnSelectedItemQuickUseToggle;
        materialButton.ButtonGroup.Pressed += OnItemCategoryPress;
        PlayerInventoryManager.Updated += OnInventoryUpdate;

        PopulateSlots(currentCategory);
        sfxOpen.Play(); 
    }

    public override void _ExitTree()
    {
        var player = this.GetPlayer();
        if (player is null) return;

        slots.ForEach(slot => slot.Pressed -= SelectSlot);
        closeButton.Pressed -= Close;
        selectedItemActionButton.Toggled -= OnActionButtonToggle;
        materialButton.ButtonGroup.Pressed -= OnItemCategoryPress;
        PlayerInventoryManager.Updated -= OnInventoryUpdate;
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
        sfxClick.Play();
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
        
        selectedItemQuickUseButton.Visible = item?.Item.ItemCategory == Item.Category.Consumable;
        selectedItemQuickUseButton.ButtonPressed = item?.Item.Id == PlayerInventoryManager.QuickSlotItem?.Id;
        selectedItemQuickUseButton.Text = selectedItemQuickUseButton.ButtonPressed ? "Unequip" : "Quick Use";
    }

    private void OnItemCategoryPress(BaseButton button)
    {
        var meta = button.GetMeta("Category").AsString();
        var category = Enum.Parse<Item.Category>(meta);

        if (currentCategory == category) return;
        sfxClick.Play();
        currentCategory = category;
    
        PopulateSlots(category);
    }

    private void PopulateSlots(Item.Category category)
    {
        var player = this.GetPlayer();

        if (player is null) return;

        var items = PlayerInventoryManager.GetItemsFromCategory(category);

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
            sfxClick.Play();
            UpdateSelectedItem(slot.Item);
        }
    }

    private void OnActionButtonToggle(bool pressed)
    {
        
        var player = this.GetPlayer();

        if (selectedItem is null || selectedItem.ItemCategory != Item.Category.Weapon || player is null) return;

        if (pressed)
        {
            sfxClick.Play();
            player.WeaponManager.Equip(selectedItem);
            selectedItemActionButton.Text = "Unequip";
        }
        else if (selectedItem.Id == player.WeaponManager.Weapon?.Id)
        {
            sfxClick.Play();
            player.WeaponManager.Unequip();
            selectedItemActionButton.Text = "Equip";
        }
    }

    private void OnActionButtonPress()
    {
        var player = this.GetPlayer();

        if (selectedItem is null || selectedItem.ItemCategory != Item.Category.Consumable || player is null) return;
        sfxClick.Play();
        PlayerInventoryManager.UseItem(selectedItem);
    }

    private void OnSelectedItemQuickUseToggle(bool pressed)
    {
       
        var player = this.GetPlayer();

        if (selectedItem is null || selectedItem.ItemCategory != Item.Category.Consumable || player is null) return;

        if (pressed)
        {
            sfxClick.Play();
            selectedItemQuickUseButton.Text = "Unequip";
            PlayerInventoryManager.SetQuickSlotItem(selectedItem);
        }
        else if (selectedItem.Id == PlayerInventoryManager.QuickSlotItem?.Id)
        {
            sfxClick.Play();
            selectedItemQuickUseButton.Text = "Quick Use";
            PlayerInventoryManager.SetQuickSlotItem(null);
        }
    }

    public override void Close()
    {
        base.Close();
        sfxClose.Play(); 
    }
}
