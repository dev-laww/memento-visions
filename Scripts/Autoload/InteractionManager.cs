using System;
using System.Collections.Generic;
using System.Linq;
using Game.Common.Interfaces;
using Game.Utils.Extensions;
using Godot;

namespace Game.Autoload;

public partial class InteractionManager : Autoload<InteractionManager>
{
    private readonly List<IInteractable> areas = [];
    private IInteractable lastClosest;

    public override void _Process(double delta)
    {
        if (areas.Count == 0) return;

        var closest = GetClosest();

        if (closest == lastClosest || closest == null) return;

        areas.Where(area => area != closest).ToList().ForEach(area => area.HideUI());

        lastClosest = closest;

        closest.ShowUI();
    }

    public override void _Input(InputEvent @event)
    {
        if (!@event.IsActionPressed("interact") || areas.Count == 0) return;

        var closest = GetClosest();

        closest.Interact();
    }

    public static void Register(IInteractable area)
    {
        if (area?.InteractionPosition == null)
        {
            GD.PrintErr("Tried to register null interactable or one with null position");
            return;
        }

        Instance.areas.Add(area);
    }


    public static void Unregister(IInteractable area)
    {
        if (area == null) return;

        if (area == Instance.lastClosest)
            Instance.lastClosest = null;

        Instance.areas.Remove(area);

        if (GodotObject.IsInstanceValid(area as GodotObject))
            area.HideUI();
    }

    private IInteractable GetClosest()
    {
        if (areas.Count == 0) return null;

        areas.RemoveAll(area =>
            area is GodotObject godotObj && !GodotObject.IsInstanceValid(godotObj)
        );

        var player = this.GetPlayer();
        if (player == null) return null;

        return areas
            .Where(area =>
                area != null &&
                GodotObject.IsInstanceValid(area as GodotObject)
            )
            .OrderBy(area => area.InteractionPosition.DistanceTo(player.GlobalPosition))
            .FirstOrDefault();
    }
}