using System.Collections.Generic;
using Godot;

namespace Game.Utils;

public static class DirAccessUtils
{
    public static List<string> GetFilesRecursively(string path, string extension = null)
    {
        var files = new List<string>();

        if (!DirAccess.DirExistsAbsolute(path))
        {
            GD.PrintErr($"Directory does not exist: {path}");
            return null;
        }

        var dir = DirAccess.Open(path);

        dir.ListDirBegin();

        var file = dir.GetNext();

        while (file != string.Empty)
        {
            if (DirAccess.DirExistsAbsolute($"{path}/{file}"))
            {
                files.AddRange(GetFilesRecursively($"{path}/{file}", extension));
            }
            else if (extension == null || file.EndsWith(extension))
            {
                files.Add($"{path}/{file}");
            }

            file = dir.GetNext();
        }

        dir.ListDirEnd();

        return files;
    }
}