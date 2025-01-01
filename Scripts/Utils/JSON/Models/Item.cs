using System.Text.Json.Serialization;
using Godot;
using Resource = Game.Resources.Item;

namespace Game.Utils.JSON.Models;

public class Item
{
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("unique_name")] public string UniqueName { get; set; }

    [JsonPropertyName("resource")] public string Resource { get; set; }

    [JsonPropertyName("scene")] public string Scene { get; set; }

    public Resource ToItem() => GD.Load<Resource>(Resource);
}