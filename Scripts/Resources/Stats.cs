using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass]
public partial class Stats : Resource
{
    [Export] public float MaxHealth { get; set; } = 100;
    [Export] public float MaxMana { get; set; } = 100;
    [Export] public float Attack { get; set; } = 10;
    [Export] public float Magic { get; set; } = 10;
    [Export] public float Defense { get; set; } = 10;
    [Export] public float Speed { get; set; } = 50;

    [ExportCategory("Resistance Multipliers")]
    [Export]
    public float PhysicalDamageMultiplier { get; set; } = 1.0f;

    [Export] public float MagicalDamageMultiplier { get; set; } = 1.0f;
}