using Game.Components.Managers;
using Game.Resources;
using Game.UI;
using Godot;
using Game.Utils.Extensions;
using GodotUtilities;

namespace Game.Quests;

[Tool]
[GlobalClass]
public partial class CollectItemObjectives : QuestObjectives
{
    [Export] public string itemUniqueName { get; set; }
    private InventoryManager InventoryManager;

    public override void _Ready()
    {
        base._Ready();
        currentCount = 0; 
        
        var playerInventory = this.GetPlayer()?.Inventory;
        if (playerInventory == null)
        {
            GD.PrintErr("Player inventory not found!");
            return;
        }
        
        playerInventory.ItemAdd += OnItemAdded;
        playerInventory.ItemRemove += OnItemRemoved;
        CheckExistingItems(playerInventory);
    }

    private void CheckExistingItems(InventoryManager inventory)
    {
   
        var existingItems = inventory.GetItemsByUniqueName(itemUniqueName);
        foreach (var item in existingItems)
        {
            currentCount += item.Value;
        }
        UpdateProgress();
    }

    private void OnItemAdded(Item item)
    {
        if (item.UniqueName != itemUniqueName) return;
        if (currentCount >= TargetCount) return;

        currentCount += item.Value;
        UpdateProgress();
    }

    private void OnItemRemoved(Item item)
    {
        if (item.UniqueName != itemUniqueName) return;

        currentCount -= item.Value;
        if (currentCount < 0) currentCount = 0;
      
        UpdateProgress();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        var playerInventory = this.GetPlayer()?.Inventory;
        if (playerInventory != null)
        {
            playerInventory.ItemAdd -= OnItemAdded;
            playerInventory.ItemRemove -= OnItemRemoved;
        }
    }

    
}