using Godot;
using GodotUtilities;

namespace Game;

[Tool]
[Scene]
public partial class RegenNumber : Node2D
{
    [Export(PropertyHint.MultilineText)]
    public string Text
    {
        get => label.Text;
        set
        {
            if (label == null) return;

            label.Text = value;

            if (value == string.Empty)
            {
                label.Size = new Vector2(1, 8);
                label.Position = new Vector2(0, -4);
            }

            label.NotifyPropertyListChanged();
        }
    }

    [Node]
    private AnimationPlayer animationPlayer;
    
    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    private Label label => GetNodeOrNull<Label>("Label");
    
    
}