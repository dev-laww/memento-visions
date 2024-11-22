using System.Text.Json.Serialization;

namespace Game.Utils.JSON.Models;

public class RecipeItem
{
    [JsonPropertyName("item")]
    public string Item { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}