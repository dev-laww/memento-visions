using System.Collections.Generic;
using System.Linq;
using Game.Common;
using Game.Registry;
using Game.Resources;
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
        player.InventoryManager.Updated += _ => Reset();

        PopulateSlots();
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
        selectedItemIcon.Texture = item?.Item.Icon;
        selectedItemName.Text = item?.Item.Name;
        selectedItemCategory.Text = item?.Item.ItemCategory.ToString();

        selectedItemDescription.Text = item?.Item.Description;

        selectedRecipe = item is not null ? RecipeRegistry.Get(item.Item.Id) : null;
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

        Reset();
    }

    private void Reset()
    {
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

        return ingredients != null && ingredients.All(player.InventoryManager.HasItem);
    }

    private void Create(int qty = 1)
    {
        var player = this.GetPlayer();

        if (player is null || !CanCreate(qty) || selectedRecipe is null) return;

        var ingredients = selectedRecipe.GetIngredients(qty);

        foreach (var ingredient in ingredients)
            player.InventoryManager.RemoveItem(ingredient);

        var item = new ItemGroup
        {
            Item = selectedRecipe.Result.Item,
            Quantity = selectedRecipe.Result.Quantity * qty
        };

        player.InventoryManager.AddItem(item);
        Log.Debug($"Created {item}.");
    }
}