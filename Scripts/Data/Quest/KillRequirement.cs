﻿using Godot;

namespace Game.Data;

[Tool]
[GlobalClass]
public partial class KillRequirement : Resource
{
    [Export] public string Id;
    [Export] public int Amount;
    public int Quantity;
    
    public override string ToString() => $"<KillRequirement ({Id} x{Amount})>";
}