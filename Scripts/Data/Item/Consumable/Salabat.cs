using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
public partial class Salabat : Item
{
    public override void Use(Entity entity)
    {
        entity.StatsManager.Heal(15, mode: StatsManager.ModifyMode.Percentage);
    }
}