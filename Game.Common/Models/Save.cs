using System.ComponentModel;
using Newtonsoft.Json;

namespace Game.Common.Models;

public class Save
{
    [JsonProperty("level")] public float Level { get; private set; } = 1;
    [JsonProperty("experience")] public float Experience { get; private set; }
    [JsonProperty("items")] public List<Item> Items { get; private set; } = [];
    [JsonProperty("equipped")] public string Equipped { get; private set; } = string.Empty;
    [JsonProperty("quick_slot")] public string QuickSlotItem { get; private set; } = string.Empty;

    [JsonProperty("current_chapter", DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue("prologue_0.1")]
    public string CurrentChapter { get; private set; }

    [JsonProperty("quests")] private List<string> quests = [];

    public void SetItems(List<Item> items) => Items = items;
    public void SetLevel(float level) => Level = level;
    public void SetExperience(float experience) => Experience = experience;
    public void SetQuickSlotItem(string item) => QuickSlotItem = item;
    public void SetEquipped(string item) => Equipped = item;

    public IReadOnlyList<string> GetQuests() => quests;

    public void AddQuest(string quest)
    {
        if (quests.Contains(quest)) return;
        quests.Add(quest);
    }

    public void RemoveQuest(string quest)
    {
        if (!quests.Contains(quest)) return;
        quests.Remove(quest);
    }

    public void SetQuests(List<string> quests)
    {
        this.quests = quests;
    }

    public void SetCurrentChapter(string chapter)
    {
        CurrentChapter = chapter;
    }
}