using System.Text.Json;
using Godot;
using FileAccess = Godot.FileAccess;


namespace Game.Utils.JSON;

// TODO: add encryption, and consider using a different serializer (Newtonsoft.Json)

public class JSON
{
    public static T Load<T>(string path)
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        if (file != null) return JsonSerializer.Deserialize<T>(file.GetAsText());

        GD.PrintErr($"File not found: {path}");
        return default;
    }

    public static void Save<T>(string path, T data)
    {
        var json = JsonSerializer.Serialize(data);

        var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        if (file == null)
        {
            GD.PrintErr($"Failed to open file: {path}");
            return;
        }

        file.StoreString(json);
    }
}