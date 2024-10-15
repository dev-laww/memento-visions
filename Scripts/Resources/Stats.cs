using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass]
public partial class Stats: Resource
{
    [Export]
    public int Health { get; set; } = 100;

    [Export]
    public int MaxHealth { get; set; } = 100;
    
    [Export]
    public int MaxStamina { get; set; } = 100;

    [Export]
    public int Mana { get; set; } = 100;

    [Export]
    public int MaxMana { get; set; } = 100;

    [Export]
    public int Attack { get; set; } = 10;

    [Export]
    public int Defense { get; set; } = 10;

    [Export]
    public int Speed { get; set; } = 50;
}