using Game.Components;
using Game.Entities;
using Godot;

namespace Game.Data;

[Tool]
public partial class BananaCue : Item
{
    public override void Use(Entity entity)
    {
        entity.StatsManager.Heal(10, mode: StatsManager.ModifyMode.Percentage);
    }
}