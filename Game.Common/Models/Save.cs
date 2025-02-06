using Newtonsoft.Json;

namespace Game.Common.Models;

public class Save
{
    [JsonProperty("player")] public Player Player { get; private set; } = new();
    [JsonProperty("items")] public List<Item> Items { get; private set; } = [];

    public void SetItemsData(List<Item> items) => Items = items;
    public void SetPlayerData(Player player) => Player = player;
}