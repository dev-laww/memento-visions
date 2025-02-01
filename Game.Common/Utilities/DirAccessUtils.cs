using Godot;

namespace Game.Common.Utilities;

public static class DirAccessUtils
{
    public static List<string> GetFilesRecursively(string path, string? extension = null)
    {
        var files = new List<string>();

        if (!DirAccess.DirExistsAbsolute(path))
        {
            GD.PrintErr($"Directory does not exist: {path}");
            return [];
        }

        var dir = DirAccess.Open(path);

        dir.ListDirBegin();

        var file = dir.GetNext().TrimSuffix(".remap");

        while (file != string.Empty)
        {
            if (DirAccess.DirExistsAbsolute($"{path}/{file}"))
            {
                files.AddRange(GetFilesRecursively($"{path}/{file}", extension) ?? []);
            }
            else if (extension == null || file.EndsWith(extension))
            {
                files.Add($"{path}/{file}");
            }

            file = dir.GetNext().TrimSuffix(".remap");
        }

        dir.ListDirEnd();

        return files;
    }

    public static List<string> GetFilesFromDirectories(params string[] paths)
    {
        var files = new List<string>();

        foreach (var path in paths)
            files.AddRange(GetFilesRecursively(path) ?? []);

        return files;
    }
}