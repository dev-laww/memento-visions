using System.Collections.Generic;
using System.Linq;
using Game.Resources;
using Game.UI.Common;
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

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        slots = slotsContainer.GetChildrenOfType<Slot>().ToList();
        slots.ForEach(slot => slot.Pressed += OnSlotPress);
        closeButton.Pressed += Close;
        materialButton.ButtonGroup.Pressed += OnItemCategoryPress;
    }

    private void OnSlotPress(Slot slot)
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
        selectedItemQuantity.Text = item?.Quantity > 999 ? "999+" : item?.Quantity.ToString();
        selectedItemDescription.Text = item?.Item.Description;
        
        selectedItemActionButton.Visible = item?.Item.ItemCategory is Item.Category.Material or Item.Category.Consumable;
        selectedItemActionButton.Text = item?.Item.ItemCategory switch
        {
            Item.Category.Material => "Craft",
            Item.Category.Consumable => "Use",
            _ => string.Empty
        };
    }

    // TODO: Implement me
    private void OnItemCategoryPress(BaseButton button)
    {
        // var meta = button.GetMeta("Category").AsString();
        // var category = Enum.Parse<Item.Category>(meta);
        //
        // slots.ForEach(slot => slot.Item = null);
        //
        // var items = PlayerInventoryManager.GetItemsFromCategory(category);
        //
        // slots.Where(slot => slot.Item != null).ToList().ForEach(slot => slot.Item = null);
        //
        // for (var i = 0; i < items.Count; i++)
        // {
        //     slots[i].Item = items[i];
        // }
    }
}