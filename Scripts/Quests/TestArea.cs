using Godot;
using System;
using Game.Entities;
using Game.Entities.Player;
using Game.Utils.Extensions;
using GodotUtilities;

namespace Game.Quests;
[Scene]
public partial class TestArea : Node2D
{
    private Player player;
    private NPC npc;

    public override void _Ready()
    {
        base._Ready();
        WireNodes();
        player = this.GetPlayer();
    }
}