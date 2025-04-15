using Godot;

namespace Game.Data;

[GlobalClass]
public partial class EntityDetail : Resource
{
    public enum EntityType
    {
        NPC,
        Enemy
    }

    [Export] public SpriteFrames Frames;
    [Export] public string Name;
    [Export] public EntityType Type;
    [Export(PropertyHint.MultilineText)] public string Description;
}
