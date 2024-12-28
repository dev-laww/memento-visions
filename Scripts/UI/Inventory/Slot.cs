using System.Linq;
using Game.Resources;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Inventory;

[Tool]
[Scene]
public partial class Slot : Control
{
    [Export]
    public bool Selected
    {
        get => _selected;
        private set
        {
            _selected = value;

            if (value)
                Select();
            else
                Deselect();

            NotifyPropertyListChanged();
        }
    }

    [Node]
    public Button button;

    [Node]
    private Label label;

    [Node]
    private TextureRect icon;

    [Node]
    private AnimationPlayer animationPlayer;

    [Export]
    public Item Item
    {
        get => item;
        set
        {
            item = value;

            if (Engine.IsEditorHint()) return;

            if (item == null)
            {
                label.Visible = false;
                icon.Texture = null;
                return;
            }

            if (label == null || icon == null) return;

            label.Visible = item.Value > 1;
            label.Text = item.Value > 999 ? "999+" : item.Value.ToString();
            icon.Texture = item.Icon;
        }
    }

    private Item item;
    private bool _selected;
    public bool IsOccupied => item != null;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        if (Selected) Select();

        if (Engine.IsEditorHint()) return;

        button.Pressed += Select;
        button.SetDefaultCursorShape(CursorShape.PointingHand);

        if (this.GetPlayer() == null) return;

        label.Visible = false;
        icon.Texture = null;
    }

    public void Clear()
    {
        Item = null;
        Deselect();
    }

    public void Select()
    {
        if (animationPlayer == null || !IsInsideTree()) return;

        var slots = GetTree().GetNodesInGroup<Slot>("Slots").Where(slot => slot != this).ToList();

        slots.ForEach(s => s.Deselect());
        animationPlayer.Play("select");
        _selected = true;
    }

    private void Deselect()
    {
        if (animationPlayer == null) return;

        animationPlayer.Play("RESET");
        _selected = false;
    }
}