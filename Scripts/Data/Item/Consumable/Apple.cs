using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
public partial class Apple: Item
{
    public override void Use(Entity entity)
    {
        entity.StatsManager.Heal(10);
    }
}