using System.Linq;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Common;

// [Tool]
[Scene]
public partial class Slot : Control
{
    [Export]
    public bool IsSelected
    {
        get => _selected;
        private set
        {
            if (value)
                Select();
            else
                Deselect();

            NotifyPropertyListChanged();
        }
    }

    [Node] private Button button;
    [Node] private Label label;
    [Node] private TextureRect icon;
    [Node] private AnimationPlayer animationPlayer;

    [Signal] public delegate void SelectedEventHandler(Slot slot);
    
    private bool _selected;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        button.Pressed += Select;

        if (this.GetPlayer() == null) return;

        label.Visible = false;
        icon.Texture = null;
    }

    public void Clear()
    {
        Deselect();
    }

    public void Select()
    {
        if (animationPlayer == null || !IsInsideTree()) return;

        var slots = GetTree().GetNodesInGroup<Slot>("Slots").Where(slot => slot != this).ToList();

        slots.ForEach(s => s.Deselect());
        animationPlayer.Play("select");
        _selected = true;
        EmitSignal(SignalName.Selected, this);
    }

    public void Deselect()
    {
        if (animationPlayer == null) return;

        animationPlayer.Play("RESET");
        _selected = false;
    }
}