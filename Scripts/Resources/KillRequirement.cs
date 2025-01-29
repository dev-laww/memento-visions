using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass]
public partial class KillRequirement : Resource
{
    [Export] public string Id;
    [Export] public int Amount;
    public int Quantity;
}