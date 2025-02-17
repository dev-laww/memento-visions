using System.Collections.Generic;
using System.Linq;
using Game.Common.Interfaces;
using Game.Utils.Extensions;
using Godot;

namespace Game.AutoLoad;

public partial class InteractionManager : AutoLoad<InteractionManager>
{
    private readonly List<IInteractable> areas = [];
    private IInteractable lastClosest;

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

    public static void Register(IInteractable area) => Instance.areas.Add(area);
    

    public static void Unregister(IInteractable area)
    {
        if (area == Instance.lastClosest)
            Instance.lastClosest = null;

        Instance.areas.Remove(area);
        area.HideUI();
    }

    private IInteractable GetClosest()
    {
        if (areas.Count == 0) return null;

        areas.Sort((a, b) =>
        {
            var player = this.GetPlayer();
            var aDistance = a.InteractionPosition.DistanceTo(player?.GlobalPosition ?? Vector2.Zero);
            var bDistance = b.InteractionPosition.DistanceTo(player?.GlobalPosition ?? Vector2.Zero);

            return aDistance.CompareTo(bDistance);
        });

        return areas.First();
    }
}