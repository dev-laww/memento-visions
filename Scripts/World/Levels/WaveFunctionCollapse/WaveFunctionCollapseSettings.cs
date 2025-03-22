using Godot;

namespace Game.Levels;

[Tool]
[GlobalClass]
public partial class WaveFunctionCollapseSettings : Resource
{
    [Export] public int CellSize = 256;
    [Export] public Vector2I GridSize = new(32, 32);
    [Export] public WaveFunctionCollapseEntry[] Entries;
}