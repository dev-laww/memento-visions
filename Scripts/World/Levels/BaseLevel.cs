using System;
using System.Buffers.Text;
using Game.Autoload;
using Godot;

namespace Game.World;

[GlobalClass]
public partial class BaseLevel : Node2D
{
    [Export] public string Id;

    public sealed override void _EnterTree()
    {
        var base64Path = Convert.ToBase64String(GetSceneFilePath().ToUtf8Buffer());
        SaveManager.SetCurrentChapter(base64Path);
        SaveManager.Save();
    }
}