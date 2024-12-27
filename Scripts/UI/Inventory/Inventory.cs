using Game.Components.Managers;
using Game.Resources;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class Inventory : Control
{
    private GridContainer gridContainer;
    private Slot slot;
    private PlayerInventory playerInventory;
    
    public override void _Notification(int what) 
    {
        if (what != NotificationSceneInstantiated) return;
       
        WireNodes();
    }
    
    public override void _Ready() 
    {
        gridContainer = GetNode<GridContainer>("MarginContainer/VBoxContainer/Main/NinePatchRect/MarginContainer/HBoxContainer/ScrollContainer/GridContainer");
        slot = GetNode<Slot>("MarginContainer/VBoxContainer/Main/NinePatchRect/MarginContainer/HBoxContainer/ScrollContainer/GridContainer/Slot");
       
    
        playerInventory = new PlayerInventory();
        playerInventory._Ready();
       
        if (gridContainer == null || slot == null) 
        {
            GD.PrintErr("GridContainer or Slot not found.");
            return;
        }
        else
        {
            GD.Print("GridContainer and Slot found.");
        }

        playerInventory.PrintInventory();
        foreach (var item in playerInventory.Inventory)
        {
            AddItem(item);
        }
    }
    
    //ADDING ITEMS INTO SLOTS
    
    public void AddItem(Item item)
    {
        foreach (var child in gridContainer.GetChildren())
        {
            if (child is Slot slot && !slot.IsOccupied)
            {
                slot.IsOccupied = true;
                slot.button.Icon = item.Icon;
                break;
            }
        }
    }
}