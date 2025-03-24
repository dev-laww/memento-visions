using System.Collections.Generic;
using Game.Autoload;
using Godot;
using GodotUtilities;
using Game.Data;
using Game.Utils.Extensions;

namespace Game.World;

[Tool]
[Scene]
[Icon("res://assets/icons/item-component.svg")]
public partial class Item : Node2D
{
    [Export]
    public ItemGroup ItemGroup
    {
        get => itemGroup;
        set
        {
            itemGroup = value;

            if (value is not null) value.PropertyListChanged += Initialize;

            Initialize();
        }
    }

    [Node] private Sprite2D Sprite;

    private ItemGroup itemGroup;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!IsNodeReady() || Sprite is null) return;

        Sprite.Texture = ItemGroup?.Item?.Icon ?? ResourceLoader.Load<Texture2D>("res://assets/items/unknown.png");
        Sprite.NotifyPropertyListChanged();

        UpdateConfigurationWarnings();
        NotifyPropertyListChanged();
    }

    private void OnBodyEntered(Node _)
    {
        var player = this.GetPlayer();
        if (player is null || Engine.IsEditorHint() || ItemGroup is null) return;

        PlayerInventoryManager.AddItem(ItemGroup);
        QueueFree();
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (ItemGroup is null)
            warnings.Add("ItemGroup is not set.");

        if (ItemGroup is not null && ItemGroup.Item is null)
            warnings.Add("Item is not set.");

        if (ItemGroup is not null && ItemGroup.Quantity <= 0)
            warnings.Add("Quantity is less than or equal to 0.");

        return [.. warnings];
    }
}