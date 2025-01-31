using Godot;

namespace Game.UI.Common;

[Tool]
public partial class InteractionUI : Control
{
    public string Text
    {
        get => Label.Text;
        set
        {
            Label.Text = value;
            Label.NotifyPropertyListChanged();
        }
    } 

    private Label Label => GetNode<Label>("%Label");
}
