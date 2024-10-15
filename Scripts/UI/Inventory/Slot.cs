using System.Collections.Generic;
using Game.Resources;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
[Tool]
public partial class Slot : Control
{
    [Export]
    public Item Item
    {
        get => resource;
        set
        {
            resource = value;
            UpdateConfigurationWarnings();

            if (icon == null) return;

            icon.Texture = resource?.Icon;

            if (icon.Texture == null) return;

            var size = icon.Texture.GetSize();
            var scale = new Vector2(32, 32) / size;
            icon.Scale = scale;
        }
    }

    [Node]
    private Sprite2D icon;

    private Item resource;
    private int quantity;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }


    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (resource == null)
            warnings.Add("Item is not set");

        return warnings.ToArray();
    }
}