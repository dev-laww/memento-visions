using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
public partial class MoonlitElixir : Item
{
    public override void Use(Entity entity)
    {
        entity.StatsManager.Cleanse();
    }
}