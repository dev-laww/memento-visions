using System.Linq;
using Game.Entities.Characters;
using Godot;

namespace Game.Utils.Extensions;

#nullable enable
public static class NodeExtensions
{
    public static Player? GetPlayer(this Node node)
    {
        var player = node.GetTree().GetNodesInGroup("Player").FirstOrDefault();

        return player as Player;
    }
}