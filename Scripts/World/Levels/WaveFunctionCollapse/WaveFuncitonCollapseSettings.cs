using Godot;

namespace Game.Levels;

[GlobalClass]
public partial class WaveFuncitonCollapseSettings : Resource
{
    [Export] public Vector2I gridSize = new(32, 32);
    [Export] public WaveFunctionCollapseEntry[] Entries;
}