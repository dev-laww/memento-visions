using Godot;
using System;
using MonoCustomResourceRegistry;

namespace Game.Inventory;
[Tool]
[RegisteredType(nameof(InventorySlot),"",nameof(Resource))]
public partial class InventorySlot : Resource
{
    [Export]
    public String Name { get; set; } = "";
    [Export]
    public Texture2D Texture { get; set; } = new Texture2D();

    public InventorySlot()
    {
        Name = "";
        Texture = new Texture2D();
    }
}
