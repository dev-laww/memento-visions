using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Autoload;
using Game.Common;
using Game.Data;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class Crafting : Overlay
{
    [Node] private ResourcePreloader resourcePreloader;
    [Node] private TextureButton closeButton;
    [Node] private GridContainer slotsContainer;

    [Node] private TextureRect selectedItemIcon;
    [Node] private Label selectedItemName;
    [Node] private Label selectedItemCategory;
    [Node] private RichTextLabel selectedItemDescription;

    [Node] private Label quantityInput;
    [Node] private TextureButton increaseButton;
    [Node] private TextureButton decreaseButton;
    [Node] private Button craftButton;
    [Node] private HBoxContainer quantityControl;

    [Signal] public delegate void ItemCraftedEventHandler();

    private List<Slot> slots;
    private Recipe selectedRecipe;
    private int quantity = 1;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        var player = this.GetPlayer();
        if (player is null) return;

        slots = slotsContainer.GetChildrenOfType<Slot>().ToList();

        slots.ForEach(slot => slot.Pressed += SelectSlot);
        closeButton.Pressed += Close;
        increaseButton.Pressed += OnIncreaseButtonPress;
        decreaseButton.Pressed += OnDecreaseButtonPress;
        craftButton.Pressed += OnCraftButtonPress;
        PlayerInventoryManager.Updated += _ => Reset();

        PopulateSlots();
        Reset();
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        PlayerInventoryManager.Updated -= _ => Reset();
    }


    private void PopulateSlots()
    {
        var recipes = RecipeRegistry.GetRecipes(Recipe.Type.Craftable);


        for (var i = 0; i < recipes.Count; i++)
        {
            var slot = slots[i];
            var recipe = recipes[i];

            slot.Item = recipe.Result;
        }

        Reset();
    }

    private void SelectSlot(Slot slot)
    {
        var selectedSlot = slots.FirstOrDefault(s => s.Selected);

        if (selectedSlot is null)
        {
            slot.Selected = true;
            return;
        }

        if (selectedSlot == slot) return;

        slot.Selected = true;
        selectedSlot.Selected = false;
        UpdateSelectedItem(slot.Item);
    }

    private void UpdateSelectedItem(ItemGroup item)
    {
        selectedRecipe = item is not null ? RecipeRegistry.Get(item.Item.Id) : null;
        selectedItemIcon.Texture = item?.Item.Icon;
        selectedItemName.Text = item?.Item.Name;
        selectedItemCategory.Text = item?.Item.ItemCategory.ToString();

        selectedItemDescription.Text = new StringBuilder().AppendLine(item?.Item.Description)
            .AppendLine()
            .AppendLine("Ingredients:")
            .AppendLine()
            .AppendJoin(
                "\n",
                selectedRecipe?.GetIngredients().Select(ingredient =>
                    $"{ingredient.Quantity}x {ingredient.Item.Name}") ?? Array.Empty<string>()
            )
            .ToString();

        quantity = 1;
        quantityInput.Text = $"{(selectedRecipe?.Result.Quantity ?? 0) * quantity}";
        UpdateButtonState();
    }

    private void OnIncreaseButtonPress()
    {
        if (selectedRecipe is null) return;

        quantity++;
        quantityInput.Text = $"{(selectedRecipe?.Result.Quantity ?? 0) * quantity}";
        UpdateButtonState();
    }

    private void OnDecreaseButtonPress()
    {
        if (selectedRecipe is null) return;

        if (quantity <= 0) return;

        quantity--;
        quantityInput.Text = $"{(selectedRecipe?.Result.Quantity ?? 0) * quantity}";
        UpdateButtonState();
    }

    private void OnCraftButtonPress()
    {
        if (selectedRecipe is null) return;

        Create(quantity);
        EmitSignalItemCrafted();
        Close();
    }

    private void Reset()
    {
        if (!IsInstanceValid(this)) return;

        quantity = 1;
        quantityInput.Text = $"{(selectedRecipe?.Result.Quantity ?? 0) * quantity}";

        var firstSlot = slots.First();
        SelectSlot(firstSlot);
        UpdateSelectedItem(firstSlot.Item);
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        quantityControl.Visible = selectedRecipe is not null;
        craftButton.Visible = selectedRecipe is not null;

        craftButton.Disabled = selectedRecipe is null || !CanCreate(quantity);
        craftButton.Text = CanCreate(quantity) ? "Craft" : "Not enough resources";

        increaseButton.Disabled = selectedRecipe is null || !CanCreate(quantity + 1);
        decreaseButton.Disabled = quantity <= 1;
    }

    public override void Close()
    {
        base.Close();

        Reset();
    }

    private bool CanCreate(int qty = 1)
    {
        var player = this.GetPlayer();

        if (player is null) return false;

        var ingredients = selectedRecipe?.GetIngredients(qty);

        return ingredients != null && ingredients.All(PlayerInventoryManager.HasItem);
    }

    private void Create(int qty = 1)
    {
        if (!CanCreate(qty) || selectedRecipe is null) return;

        var recipe = selectedRecipe.Duplicate() as Recipe;

        var ingredients = recipe.GetIngredients(qty);

        foreach (var ingredient in ingredients)
            PlayerInventoryManager.RemoveItem(ingredient);

        var item = new ItemGroup
        {
            Item = recipe.Result.Item,
            Quantity = recipe.Result.Quantity * qty
        };

        PlayerInventoryManager.AddItem(item);
        Reset();
        Log.Debug($"Created {item}.");
    }
}