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
    [JsonProperty("frenzy_mode_unlocked")] public bool FrenzyModeUnlocked { get; private set; }
    [JsonProperty("unlocked_recipes")] public List<string> UnlockedRecipes { get; private set; } = [];
    [JsonProperty("npcs_encountered")] public List<string> NpcsEncountered { get; private set; } = [];
    [JsonProperty("game_intro_shown")] public bool IntroShown { get; private set; }
    [JsonProperty("enemy_details")] public List<string> EnemyDetails { get; private set; } = [];

    [JsonProperty("current_chapter", DefaultValueHandling = DefaultValueHandling.Populate),
     DefaultValue("cmVzOi8vU2NlbmVzL1dvcmxkL0xldmVscy9TdG9yeS9Qcm9sb2d1ZS9Qcm9sb2d1ZUJhci50c2Nu")]
    public string CurrentChapter { get; private set; } = "cmVzOi8vU2NlbmVzL1dvcmxkL0xldmVscy9TdG9yeS9Qcm9sb2d1ZS9Qcm9sb2d1ZUJhci50c2Nu";

    [JsonProperty("quests")] private List<Quest> quests = [];

    public void SetItems(List<Item> items) => Items = items;
    public void SetLevel(float level) => Level = level;
    public void SetExperience(float experience) => Experience = experience;
    public void SetQuickSlotItem(string item) => QuickSlotItem = item;
    public void SetEquipped(string item) => Equipped = item;

    public IReadOnlyList<Quest> GetQuests() => quests;
    public IReadOnlyList<string> GetEnemyDetails() => EnemyDetails;

    public void AddQuest(Quest quest)
    {
        if (quests.Any(q => q.Id == quest.Id)) return;
        quests.Add(quest);
    }

    public void SetQuests(List<Quest> quests)
    {
        this.quests = quests;
    }

    public void SetCurrentChapter(string chapter)
    {
        CurrentChapter = chapter;
    }

    public void UnlockFrenzyMode()
    {
        FrenzyModeUnlocked = true;
    }

    public void UnlockRecipe(string recipe)
    {
        if (UnlockedRecipes.Contains(recipe)) return;
        UnlockedRecipes.Add(recipe);
    }

    public void SetNpcsEncountered(List<string> npcs)
    {
        NpcsEncountered = npcs;
    }

    public void AddNpcsEncountered(string npc)
    {
        if (NpcsEncountered.Contains(npc)) return;
        NpcsEncountered.Add(npc);
    }

    public void SetIntroShown(bool shown)
    {
        IntroShown = shown;
    }

    public void AddEnemyDetails(string enemyId)
    {
        if (EnemyDetails.Contains(enemyId)) return;
        EnemyDetails.Add(enemyId);
    }
}