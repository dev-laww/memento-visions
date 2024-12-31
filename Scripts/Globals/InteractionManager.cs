using System.Collections.Generic;
using System.Linq;
using Game.Components.Area;
using Game.Utils.Extensions;
using Godot;

namespace Game.Globals;

public partial class InteractionManager : Global<InteractionManager>
{
    private readonly List<Interaction> areas = new();
    private Interaction lastClosest;

    public override void _Process(double delta)
    {
        if (areas.Count == 0) return;

        var closest = GetClosest();

        if (closest == lastClosest || closest == null) return;

        areas.ForEach(area => area.HideUI());

        lastClosest = closest;

        closest.ShowUI();
    }

    public override void _Input(InputEvent @event)
    {
        if (!@event.IsActionPressed("interact") || areas.Count == 0) return;

        var closest = GetClosest();

        closest.Interact();
    }

    public static void Register(Interaction area) => Instance.areas.Add(area);
    

    public static void Unregister(Interaction area)
    {
        if (area == Instance.lastClosest)
            Instance.lastClosest = null;

        Instance.areas.Remove(area);
        area.HideUI();
    }

    private Interaction GetClosest()
    {
        if (areas.Count == 0) return null;

        areas.Sort((a, b) =>
        {
            var player = this.GetPlayer();
            var aDistance = a.GlobalPosition.DistanceTo(player.GlobalPosition);
            var bDistance = b.GlobalPosition.DistanceTo(player.GlobalPosition);

            return aDistance.CompareTo(bDistance);
        });

        return areas.First();
    }
}