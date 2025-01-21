using Game.Resources;
using Godot;
using GodotUtilities;

namespace Game.UI.Common;

// [Tool]
[Scene]
public partial class Slot : Panel
{
    [Node] private Label label;
    [Node] private TextureRect icon;
    [Node] private AnimationPlayer animationPlayer;

    [Signal] public delegate void UpdatedEventHandler(ItemGroup item);
    [Signal] public delegate void PressedEventHandler(Slot slot);

    private bool selected;
    private ItemGroup item;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            animationPlayer.Play(selected ? "select" : "RESET");
        }
    }

    public ItemGroup Item
    {
        get => item;
        set
        {
            item = value;
            UpdateSlot();
        }
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        UpdateSlot();
        GuiInput += OnGuiInput;
    }

    private void UpdateSlot()
    {
        label.Visible = item is not null && item.Quantity > 1;
        icon.Texture = item?.Item.Icon;
        label.Text = item?.Quantity > 999 ? "999+" : item?.Quantity.ToString();

        EmitSignal(SignalName.Updated, item);
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseAction) return;

        if (!mouseAction.Pressed) return;

        animationPlayer.Play(selected ? "select" : "RESET");

        EmitSignal(SignalName.Pressed, this);
    }
}