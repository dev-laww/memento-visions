using Godot;
using Game.Entities;
using Game.Entities.Player;
using Game.Utils.Extensions;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Bar : Node2D
{
    private Player player;
    private NPC npc;

    public override void _Ready()
    {
        base._Ready();
        WireNodes();

        // Retrieve Player immediately (likely works fine)
        player = this.GetPlayer();
        // npc = GetNode<NPC>("/root/GameManager/CurrentScene/Bar/NPC");
    }
}