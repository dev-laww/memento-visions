using System.Collections.Generic;
using Newtonsoft.Json;

namespace Game.Utils.Json.Models;

public class InventoryData
{
    public class Item
    {
        [JsonProperty("unique_name")] public string UniqueName;
        [JsonProperty("amount")] public int Amount;
    }

    [JsonProperty("items")] public List<Item> Items = [];
    [JsonProperty("equipped")] public string Equipped;
}