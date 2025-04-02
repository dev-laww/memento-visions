using Godot;


namespace Game.Data;

[Tool]
[GlobalClass]
public partial class LootTable : Resource
{
    [Export]
    public ItemDrop[] Drops
    {
        get => drops;
        set
        {
            drops = value;
            NotifyPropertyListChanged();
        }
    }

    private ItemDrop[] drops;
}

