using Godot;

namespace Game.Data;

[Tool]
[GlobalClass]
public partial class ItemDrop : Resource
{
    [Export] public Item Item;
    [Export] public int Min;
    [Export] public int Max;
    [Export] public int Weight;

    public void Deconstruct(out Item item, out int min, out int max, out int weight)
    {
        item = Item;
        min = Min;
        max = Max;
        weight = Weight;
    }
}