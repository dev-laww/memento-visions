using Game.Resources;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Slot : Control
{
    [Node]
    private AnimationPlayer animationPlayer;

    [Node]
    public Button button;

    [Node]
    private Label label;

    [Node]
    private TextureRect icon;

    public Item Item
    {
        get => _item;
        set
        {
            _item = value;

            if (_item == null)
            {
                label.Visible = false;
                icon.Texture = null;
                return;
            }

            label.Visible = Item.Value > 1;
            label.Text = _item.Value.ToString();
            icon.Texture = _item.Icon;
        }
    }

    private Item _item;
    public bool IsSelected => animationPlayer.CurrentAnimation == "select";
    public bool IsOccupied => _item != null;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        button.Pressed += Select;
        button.SetDefaultCursorShape(CursorShape.PointingHand);

        label.Visible = false;
        icon.Texture = null;
    }

    public void AddToStack(Item itemToAdd) => Item += itemToAdd;

    public void Clear()
    {
        Item = null;
        Deselect();
    }

    private void Select()
    {
        GetTree().CallGroup("Slots", "Deselect");
        animationPlayer.Play("select");
    }

    private void Deselect() => animationPlayer.Play("RESET");
}