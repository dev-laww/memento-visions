using Game.Data;
using Godot;
using GodotUtilities;

namespace Game.UI.Common;

[Scene]
public partial class EnemyDetails : VBoxContainer
{
    [Node] private AnimatedSprite2D sprite;
    [Node] private Label name;
    [Node] private RichTextLabel description;

    public EntityDetail Detail
    {
        get => detail;
        set
        {
            detail = value;

            if (detail == null) return;

            sprite.SpriteFrames = detail.Frames;
            name.Text = detail.Name;
            description.Text = detail.Description;
        }
    }

    private EntityDetail detail;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}

