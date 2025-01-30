using Game.Common;
using Game.Resources;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class ItemRegistry() : Registry<Item, ItemRegistry>(Constants.ITEMS_PATH);