using Game.Utils.Json.Models;
using Godot;
using Newtonsoft.Json;
using Constants = Game.Utils.Constants;

namespace Game.Globals;

public partial class SaveManager : Global<SaveManager>
{
    public static Save Data { get; private set; }

    private static readonly string dir = OS.IsDebugBuild() ? "res://data" : "user://data";

    private static readonly string path = $"{dir}/{Constants.SAVE_NAME}";

    public static void Load()
    {
        if (FileAccess.FileExists(path))
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var content = file.GetAsText();
            file.Close();

            Data = JsonConvert.DeserializeObject<Save>(content);
        }
        else
        {
            DirAccess.MakeDirAbsolute(dir);
            Data = new Save();
        }

        Data ??= new Save();
    }

    public static void Save()
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        var json = JsonConvert.SerializeObject(Data, Formatting.Indented);

        file.StoreString(json);

        file.Close();
    }

    public static void Reset()
    {
        Data = new Save();
        Save();
    }
}