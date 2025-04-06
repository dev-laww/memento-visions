using Game.Autoload;
using Godot;

namespace Game.World;

[GlobalClass]
public partial class BaseLevel : Node2D
{
    [Export] public string Id;

    public sealed override void _EnterTree()
    {
        SaveManager.SetCurrentChapter(Id);
        SaveManager.Save();
    }
}