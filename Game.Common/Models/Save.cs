using Newtonsoft.Json;

namespace Game.Common.Models;

public class Save
{
    [JsonProperty("level")] public float Level { get; private set; } = 1;
    [JsonProperty("experience")] public float Experience { get; private set; }
    [JsonProperty("items")] public List<Item> Items { get; private set; } = [];
    [JsonProperty("equipped")] public string Equipped { get; private set; } = string.Empty;
    [JsonProperty("quick_slot")] public string QuickSlotItem { get; private set; } = string.Empty;

    public void SetItems(List<Item> items) => Items = items;
    public void SetLevel(float level) => Level = level;
    public void SetExperience(float experience) => Experience = experience;
    public void SetQuickSlotItem(string item) => QuickSlotItem = item;
    public void SetEquipped(string item) => Equipped = item;
}