using System.Linq;
using Game.Common.Extensions;
using Game.Components;
using Game.Entities;
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

    public static TelegraphCanvas? GetTelegraphCanvas(this Node node) => node.GetTree().Root.GetFirstChildOrNull<TelegraphCanvas>();
}