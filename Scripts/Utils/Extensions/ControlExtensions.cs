using System.Linq;
using Game.Entities.Player;
using Godot;
using GodotUtilities;

namespace Game.Utils.Extensions;

public static class ControlExtensions
{
    public static Player GetPlayer(this Control node)
    {
        var player = node.GetTree().GetNodesInGroup<Player>("Player").FirstOrDefault();

        return player;
    }
}