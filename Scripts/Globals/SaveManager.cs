using Game.Utils;
using Game.Utils.Json.Models;
using Godot;
using Newtonsoft.Json;
using Constants = Game.Utils.Constants;

namespace Game.Globals;

public partial class SaveManager : Global<SaveManager>
{
    private static SaveData Data;
    private static readonly string dir = $"{(OS.IsDebugBuild() ? "res" : "user")}://data";
    private static readonly string path = $"{dir}/{Constants.SAVE_NAME}";

    public static PlayerData PlayerData => Data.Player;
    public static InventoryData InventoryData => Data.Inventory;

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

    private static void Load()
    {
        Log.Info("Loading save data...");
        if (FileAccess.FileExists(path))
        {
            Log.Debug("Save data not found creating new save data...");
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var content = file.GetAsText();
            file.Close();

            Data = JsonConvert.DeserializeObject<SaveData>(content);
        }
        else
        {
            DirAccess.MakeDirAbsolute(dir);
            Data = new SaveData();
        }

        Data ??= new SaveData();
    }

    private void Save()
    {
        Log.Info("Saving data...");
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        // TODO: Implement other save data

        var inventory = InventoryManager.ToData();
        Data.Inventory = inventory;
        Data.Inventory.Equipped = WeaponManager.CurrentWeaponResource?.Id;

        var json = JsonConvert.SerializeObject(Data, Formatting.Indented);
        file.StoreString(json);
        file.Close();
    }
}