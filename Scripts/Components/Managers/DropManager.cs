using Game.Resources;
using Godot;
using GodotUtilities.Logic;

namespace Game.Components.Managers;

[Tool]
[GlobalClass]
public partial class DropManager : Node
{
    [Export] private ItemDrop[] Drops;
}