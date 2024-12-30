using System.Text.Json.Serialization;

namespace Game.Utils.JSON.Models;

public class RecipeItem
{
    [JsonPropertyName("unique_name")] public string UniqueName { get; set; }

    [JsonPropertyName("amount")] public int Amount { get; set; }
}