using Godot;

namespace Game.Levels;

[GlobalClass]
public partial class WaveFunctionCollapseEntry : Resource
{
    [Export] public PackedScene Scene;
    [Export] public int Weight;
    [Export] public PackedScene[] Up;
    [Export] public PackedScene[] Down;
    [Export] public PackedScene[] Left;
    [Export] public PackedScene[] Right;
}