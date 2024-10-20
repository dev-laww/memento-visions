using System.Collections.Generic;
using System.Linq;
using Game.Components.Area;
using Game.Utils.Extensions;
using Godot;

namespace Game.Globals;

public partial class InteractionManager : Node2D
{
    private static InteractionManager instance;

    private readonly List<Interaction> areas = new();

    private HBoxContainer ui;

    private PackedScene InteractionUIScene = GD.Load<PackedScene>("res://Scenes/UI/InteractionUI.tscn");

    public override void _Ready()
    {
        instance = this;

        ui = InteractionUIScene.Instantiate<HBoxContainer>();

        AddChild(ui);

        ui.Hide();
    }

    public override void _Process(double delta)
    {
        ui.Hide();
        
        if (areas.Count == 0) return;

        var closest = GetClosest();
        var label = ui.GetNode<Label>("Label");
        label.Text = closest.InteractionLabel;

        var icon = ui.GetNode<TextureRect>("Icon");
        var offset = new Vector2(
            (icon.Size.X + label.Size.X) / 2,
            ui.Size.Y / 2
        );

        ui.GlobalPosition = closest.Marker.GlobalPosition - offset;

        GetTree().CreateTimer(0.2).Timeout += () => ui.Show();
    }

    public override void _Input(InputEvent @event)
    {
        if (!@event.IsActionPressed("interact") || areas.Count == 0) return;

        var closest = GetClosest();

        closest.Interact();
    }

    public static void Register(Interaction area) => instance.areas.Add(area);

    public static void Unregister(Interaction area) => instance.areas.Remove(area);

    private Interaction GetClosest()
    {
        if (areas.Count == 0) return null;

        areas.Sort((a, b) =>
        {
            var player = this.GetPlayer();
            var aDistance = a.Marker.GlobalPosition.DistanceTo(player.GlobalPosition);
            var bDistance = b.Marker.GlobalPosition.DistanceTo(player.GlobalPosition);

            return aDistance.CompareTo(bDistance);
        });

        return areas.First();
    }
}