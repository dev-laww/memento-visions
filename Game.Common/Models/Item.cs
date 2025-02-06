using Newtonsoft.Json;

namespace Game.Common.Models;

public class Item
{
    [JsonProperty("unique_name")] public string Id = string.Empty;
    [JsonProperty("amount")] public int Amount;
}