using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
using Game.Resources;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI;

[Tool]
[Scene]
public partial class Crafting : Overlay
{
    [Node] private GridContainer slotsContainer;
    [Node] private ResourcePreloader resourcePreloader;

    [Node] private TextureButton closeButton;
    [Node] private Button craftButton;

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
        ClearSlots();
    }


    private bool ShouldRefreshDisplayOnPickup(Item item, string currentFilter)
    {
        return item.Type.ToString() == currentFilter || Slots.All(slot => slot.Item == null);
    }

    private void OnVisibilityChanged()
    {
        if (Engine.IsEditorHint() || Visible || player == null) return;

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
        GD.Print("Crafting button pressed");
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

        selectedItem = selectedSlot.Item;
        selectedItemIcon.Texture = selectedItem?.Icon;
        selectedItemName.Text = selectedItem?.Name;
        selectedItemType.Text = selectedItem?.Type.ToString();
        selectedItemDescription.Text = selectedItem?.Description;
    }

    private void DeselectOtherSlots(Slot selectedSlot)
    {
        var unselectedSlots = Slots.Where(s => s != selectedSlot);

        foreach (var slot in unselectedSlots)
            slot.Deselect();

        NotifyPropertyListChanged();
    }
}