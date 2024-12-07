using System.Collections.Generic;
using Game.Resources;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Slot : MarginContainer
{
    [Export]
    public Item Item;

    [Node]
    private Panel panel;

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