using Newtonsoft.Json;

namespace Game.Utils.Json.Models;

public class SaveData
{
    [JsonProperty("player")] public PlayerData Player = new();
    [JsonProperty("inventory")] public InventoryData Inventory = new();
}