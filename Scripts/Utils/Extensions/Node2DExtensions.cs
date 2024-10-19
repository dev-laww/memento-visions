using System.Linq;
using Game.Entities.Player;
using Godot;
using GodotUtilities;

namespace Game.Utils.Extensions;

public static class Node2DExtensions
{
    public static Player GetPlayer(this Node2D node)
    {
        var player = node.GetTree().GetNodesInGroup<Player>("Player").FirstOrDefault();
        
        if (player == null)
            GD.PrintErr("Player not found.");
        
        return player;
    }
}