using Game.Common;
using Game.Common.Abstract;
using Godot;

namespace Game.Data;

[GlobalClass]
public partial class ItemRegistry : Registry<Item, ItemRegistry>
{
    protected override string ResourcePath => Constants.ITEMS_PATH;
}