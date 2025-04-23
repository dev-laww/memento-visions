using System.Collections.Generic;
using Game.Common;
using Game.Common.Models;
using Godot;
using Newtonsoft.Json;

namespace Game.Autoload;

public partial class SaveManager : Autoload<SaveManager>
{
    public static Save Data { get; private set; }
    private static readonly string dir = $"{(OS.IsDebugBuild() ? "res" : "user")}://data";
    private static readonly string path = $"{dir}/{Constants.SAVE_NAME}";

    public override void _EnterTree()
    {
        Load();
        var timer = new Timer { WaitTime = OS.IsDebugBuild() ? 15 : 60, Autostart = true };
        AddChild(timer);
        timer.Timeout += Save;
    }

    public override void _ExitTree()
    {
        Save();
    }

    public static void Load()
    {
        Log.Info("Loading save data...");
        if (FileAccess.FileExists(path))
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var content = file.GetAsText();
            file.Close();

            Data = JsonConvert.DeserializeObject<Save>(content);
        }
        else
        {
            Log.Debug("Save data not found creating new save data...");
            DirAccess.MakeDirAbsolute(dir);
        }

        Data ??= new Save();
    }

    public static void Save()
    {
        Log.Info("Saving data...");
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        var json = JsonConvert.SerializeObject(Data, Formatting.Indented);
        file.StoreString(json);
        file.Close();
    }


    public static void SetItems(List<Item> items) => Data.SetItems(items);
    public static void SetLevel(float level) => Data.SetLevel(level);
    public static void SetExperience(float experience) => Data.SetExperience(experience);
    public static void SetQuickSlotItem(string item) => Data.SetQuickSlotItem(item);
    public static void SetEquipped(string item) => Data.SetEquipped(item);
    public static void SetCurrentChapter(string chapter) => Data.SetCurrentChapter(chapter);
    public static void UnlockRecipe(string itemId) => Data.UnlockRecipe(itemId);
    public static void AddNpcsEncountered(string npcId) => Data.AddNpcsEncountered(npcId);
    public static void SetQuests(List<Quest> quests) => Data.SetQuests(quests);
    public static void AddQuest(Quest quest) => Data.AddQuest(quest);
    public static void UnlockFrenzyMode() => Data.UnlockFrenzyMode();
    public static void SetIntroShown(bool shown) => Data.SetIntroShown(shown);
    public static void AddEnemyDetails(string enemyId) => Data.AddEnemyDetails(enemyId);
}