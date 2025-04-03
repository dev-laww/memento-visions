using Godot;
using GodotUtilities;

namespace Game.UI.Screens;

[Tool]
[Scene]
public partial class TextLoading : CanvasLayer
{
    [Export]
    public string Text
    {
        get {
            var pattern = @"\[(?:p|wave)[^\]]*\]([^[]+)\[/wave\]\[/p\]";

            var match = System.Text.RegularExpressions.Regex.Match(richTextLabel.Text, pattern);

            return match.Success ? match.Groups[1].Value : string.Empty;
        }
        set
        {
            richTextLabel.Text = $"[p align=center][wave amp=25.0 freq=5.0 connected=1]{value}[/wave][/p]";
            richTextLabel.NotifyPropertyListChanged();
        }
    }

    [Node] private RichTextLabel richTextLabel;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}
