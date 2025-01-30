using Newtonsoft.Json;

namespace Game.Common.Models;

public class SaveData
{
    [JsonProperty("player")] public PlayerData Player = new();
    [JsonProperty("inventory")] public InventoryData Inventory = new();
}