using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
using Game.Resources;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class Crafting : Control
{
    [Node] private GridContainer slotsContainer;
    [Node] private ResourcePreloader resourcePreloader;

    [Node] private Button closeButton;
    [Node] private Button craftButton;

    [Node] private TextureRect selectedItemIcon;
    [Node] private Label selectedItemName;
    [Node] private Label selectedItemType;
    [Node] private Label selectedItemQuantity;
    [Node] private RichTextLabel selectedItemDescription;


    private Player player => this.GetPlayer();
    private List<Slot> slots => slotsContainer.GetChildrenOfType<Slot>().ToList();
    private Item selectedItem;

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
        craftButton.Pressed += OnCraftButtonPressed;
        closeButton.Pressed += () => GetTree().CreateTimer(0.1f).Timeout += Close;
        VisibilityChanged += OnVisibilityChanged;
        slots.ForEach(slot => slot.Selected += OnSlotSelected);
        slots.First().Select();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("menu"))
        {
            Close();
            return;
        }

        if (@event.IsActionPressed("open_crafting"))
            Toggle();
    }

    private void Toggle() => Visible = !Visible;
    private void Close() => Visible = false;
    private void Clear() => slots.ForEach(slot => slot.Item = null);

    private void Reset()
    {
        Clear();
        SelectItem(null);
        slots.First().Select();
    }

    private void OnVisibilityChanged()
    {
        if (Engine.IsEditorHint() || player == null) return;

        player.SetProcessInput(!Visible);

        if (!Visible) return;

        Reset();
    }

    private void OnCraftButtonPressed()
    {
        GD.Print("Pressed");
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

    private void OnSlotSelected(Slot slot)
    {
        var unselectedSlots = slots.Where(s => s != slot).ToList();

        unselectedSlots.ForEach(s => s.Deselect());

        NotifyPropertyListChanged();
    }
}