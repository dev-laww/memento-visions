using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Game.Utils.JSON.Models;

public class Recipe
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("unique_name")]
    public string UniqueName { get; set; }

    [JsonPropertyName("result")]
    public Result Result { get; set; }

    [JsonPropertyName("ingredients")]
    public List<Ingredient> Ingredients { get; set; }

    public override string ToString() => $"{Name}: {UniqueName}";
}