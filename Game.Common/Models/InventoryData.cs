using Newtonsoft.Json;

namespace Game.Common.Models;

public class InventoryData
{
    public class Item
    {
        [JsonProperty("unique_name")] public string Id = string.Empty;
        [JsonProperty("amount")] public int Amount;
    }

    [JsonProperty("items")] public List<Item> Items = [];
    [JsonProperty("equipped")] public string Equipped = string.Empty;
}