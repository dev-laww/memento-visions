using System.Text.Json.Serialization;

namespace Game.Utils.JSON.Models;

public class Ingredient
{
    [JsonPropertyName("item")]
    public string Item { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    public override string ToString() => $"{Item}: {Amount}";
}