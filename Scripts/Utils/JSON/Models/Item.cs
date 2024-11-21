using System.Text.Json.Serialization;

namespace Game.Utils.JSON.Models;

public class Item
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("unique_name")]
    public string UniqueName { get; set; }

    [JsonPropertyName("resource")]
    public string Resource { get; set; }

    public override string ToString() => $"{Name}: {UniqueName}";
}