using System.Collections.Generic;
using System.Linq;
using Game.Entities.Player;
using Game.Globals;
using Game.Resources;
using Game.UI.Common;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

// TODO: improve popup animation
// [Tool]
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

    [Node] private LineEdit quantityInput;
    [Node] private TextureButton increaseButton;
    [Node] private TextureButton decreaseButton;
    [Node] private Button craftButton;

    private List<Slot> slots;
    private Recipe selectedRecipe;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        slots = slotsContainer.GetChildrenOfType<Slot>().ToList();

        slots.ForEach(slot => slot.Pressed += SelectSlot);
        closeButton.Pressed += Close;
        increaseButton.Pressed += OnIncreaseButtonPress;
        decreaseButton.Pressed += OnDecreaseButtonPress;
        craftButton.Pressed += OnCraftButtonPress;

        PopulateSlots();
    }

    private void PopulateSlots()
    {
        var recipes = RecipeManager.GetRecipesFromType(Recipe.Type.Craftable);


        for (var i = 0; i < recipes.Count; i++)
        {
            var slot = slots[i];
            var recipe = recipes[i];

            slot.Item = recipe.Result;
        }

        var firstSlot = slots.First();
        SelectSlot(firstSlot);
        UpdateSelectedItem(firstSlot.Item);
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

        if (item is null) return;

        selectedRecipe = RecipeManager.GetRecipeFromResult(item.Item);
        quantityInput.Text = $"{selectedRecipe?.Result.Quantity}";
    }

    private void OnIncreaseButtonPress()
    {
        if (selectedRecipe is null) return;

        var value = int.Parse(quantityInput.Text);
        quantityInput.Text = $"{value + 1}";
    }

    private void OnDecreaseButtonPress()
    {
        if (selectedRecipe is null) return;

        var value = int.Parse(quantityInput.Text);
        quantityInput.Text = $"{value - 1}";
    }

    private void OnCraftButtonPress()
    {
        if (selectedRecipe is null) return;

        var value = int.Parse(quantityInput.Text);

        if (value <= 0) return;

        selectedRecipe.Create(value);
    }
}