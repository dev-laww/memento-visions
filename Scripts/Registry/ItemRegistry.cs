using Game.Common;
using Game.Common.Abstract;
using Game.Resources;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class ItemRegistry : Registry<Item, ItemRegistry>
{
    protected override string ResourcePath => Constants.ITEMS_PATH;
}