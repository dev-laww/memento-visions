using Godot;

namespace Game.World;

[Tool]
[GlobalClass]
public partial class WaveFunctionCollapseEntry : Resource
{
    [Export] public PackedScene Scene;
    [Export] public int Weight = 1;
    [Export] public PackedScene[] Up;
    [Export] public PackedScene[] Down;
    [Export] public PackedScene[] Left;
    [Export] public PackedScene[] Right;
}