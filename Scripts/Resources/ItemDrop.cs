using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass]
public partial class ItemDrop : Resource
{
    [Export] public Item Item;
    [Export] public int MinAmount;
    [Export] public int MaxAmount;
    [Export] public int Weight;
}