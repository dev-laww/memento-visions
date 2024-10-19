using System.Collections.Generic;
using System.Linq;
using Game.Components.Area;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Globals;

[Scene]
public partial class InteractionManager : Node2D
{
    private static InteractionManager Instance { get; set; }
    private readonly List<Interaction> interactions = new();
    
    public override void _Notification(int what) {
        if (what != NotificationSceneInstantiated) return;
        
        WireNodes();
    }

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (interactions.Count == 0) return;
        
        interactions.Sort((a, b) =>
        {
            var player = this.GetPlayer();
            var aDistance = a.GlobalPosition.DistanceTo(player.GlobalPosition);
            var bDistance = b.GlobalPosition.DistanceTo(player.GlobalPosition);
            
            return aDistance.CompareTo(bDistance);
        });

        var closest = interactions.First();
        
        if (Input.IsActionPressed("interact"))
            closest.InteractWith();
    }


    public static void Register(Interaction area) => Instance.interactions.Add(area);
    
    public static void Unregister(Interaction area) => Instance.interactions.Remove(area);
}

