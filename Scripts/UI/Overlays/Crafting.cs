using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
using Game.Globals;
using Game.Resources;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

// TODO: implement craft button disabling and incrementing/decrementing item quantity disabling
[Tool]
[Scene]
public partial class Crafting : Overlay
{
    [Node] private GridContainer slotsContainer;
    [Node] private ResourcePreloader resourcePreloader;

    [Node] private TextureButton closeButton;
    [Node] private Button craftButton;

    [Node] private TextureButton increaseButton;
    [Node] private TextureButton decreaseButton;
    [Node] private LineEdit quantityInput;

    [Node] private TextureRect selectedItemIcon;
    [Node] private Label selectedItemName;
    [Node] private Label selectedItemType;
    [Node] private RichTextLabel selectedItemDescription;

    private Player player;
    private Item selectedItem;


    private List<Slot> Slots => slotsContainer.GetChildrenOfType<Slot>().ToList();

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }


    public override void _Ready()
    {
        Initialize();

        if (!ShouldInitializeGameplay()) return;

        Reset();
    }

    public override void _ExitTree()
    {
        // cleanup events
        Slots.ForEach(slot => slot.Selected -= OnSlotSelected);
    }

    private void Initialize()
    {
        player = this.GetPlayer();
        SetupEventHandlers();
    }

    private bool ShouldInitializeGameplay() => player != null && !Engine.IsEditorHint();

    private void SetupEventHandlers()
    {
        // UI Events
        quantityInput.TextChanged += OnQuantityInputTextChanged;
        increaseButton.Pressed += IncreaseQuantity;
        decreaseButton.Pressed += DecreaseQuantity;
        craftButton.Pressed += OnCraftButtonPress;
        closeButton.Pressed += HandleCloseButtonPress;
        VisibilityChanged += OnVisibilityChanged;

        // Slot Events
        InitializeSlotEvents();
    }

    private void InitializeSlotEvents()
    {
        Slots.ForEach(slot => slot.Selected += OnSlotSelected);
        Slots.First().Select();
    }

    private void HandleCloseButtonPress() => GetTree().CreateTimer(0.1f).Timeout += Close;

    private void Reset()
    {
        var items = RecipeManager.CraftableItems;

        if (!Visible) return;

        ClearSlots();

        items.ForEach(item =>
        {
            var slot = FindOrCreateSlot();
            slot.Item = item;
        });
        Slots.First().Select();
        UpdateButtonStates();
    }


    private bool ShouldRefreshDisplayOnPickup(Item item, string currentFilter)
    {
        return item.Type.ToString() == currentFilter || Slots.All(slot => slot.Item == null);
    }

    private void OnVisibilityChanged()
    {
        if (Engine.IsEditorHint() || player == null) return;

        Reset();
    }

    private Slot FindOrCreateSlot()
    {
        var existingSlot = Slots.FirstOrDefault(slot => !slot.IsOccupied);
        if (existingSlot != null) return existingSlot;

        var newSlot = resourcePreloader.InstanceSceneOrNull<Slot>();
        slotsContainer.AddChild(newSlot);
        return newSlot;
    }

    private void OnCraftButtonPress()
    {
        if (selectedItem == null || player == null) return;

        RecipeManager.CreateItem(selectedItem);

        // TODO: Add popup message

        Reset();
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

        selectedItem = selectedSlot.Item?.Duplicate();
        selectedItemIcon.Texture = selectedItem?.Icon;
        selectedItemName.Text = selectedItem?.Name;
        selectedItemType.Text = selectedItem?.Type.ToString();
        selectedItemDescription.Text = selectedItem?.Description;
        quantityInput.Text = selectedItem?.Value.ToString() ?? "0";

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (selectedItem == null)
        {
            craftButton.Disabled = true;
            decreaseButton.Disabled = true;
            increaseButton.Disabled = true;
            return;
        }

        craftButton.Disabled = !RecipeManager.CanCreateItem(selectedItem);
        decreaseButton.Disabled = selectedItem.Value <= 1;

        var nextItem = selectedItem.Duplicate();
        nextItem++;
        increaseButton.Disabled = !RecipeManager.CanCreateItem(nextItem);
    }

    private void DeselectOtherSlots(Slot selectedSlot)
    {
        var unselectedSlots = Slots.Where(s => s != selectedSlot);

        foreach (var slot in unselectedSlots)
            slot.Deselect();

        NotifyPropertyListChanged();
    }

    private void IncreaseQuantity()
    {
        if (selectedItem == null) return;

        selectedItem++;
        quantityInput.Text = selectedItem.Value.ToString();
        UpdateButtonStates();
    }

    private void DecreaseQuantity()
    {
        if (selectedItem == null) return;

        selectedItem--;
        quantityInput.Text = selectedItem.Value.ToString();
        UpdateButtonStates();
    }

    private void OnQuantityInputTextChanged(string newText)
    {
        if (selectedItem == null) return;

        if (!int.TryParse(newText, out var value)) return;

        selectedItem.Value = value;
        UpdateButtonStates();
    }
}