using Godot;

namespace Game.Resources;

[GlobalClass, Icon("res://assets/icons/recipe.svg")]
public partial class Recipe : Resource
{
    [Export] public Item Result;

    [Export]
    public int Quantity
    {
        get => quantity;
        set
        {
            quantity = Mathf.Max(1, value);

            if (Ingredients.Length == 0) return;

            foreach (var ingredient in Ingredients)
            {
                ingredient.Quantity *= quantity;
                ingredient.NotifyPropertyListChanged();
            }
            
            NotifyPropertyListChanged();
        }
    }

    [Export] public Ingredient[] Ingredients = [];

    private int quantity = 1;
}