using Godot;
using Godot.Collections;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest-step.svg")]
public partial class QuestStep : Resource
{
    public enum StepType { CollectItem, Navigate, KillEnemy, GiveItem }

    [Export]
    public StepType Type
    {
        get => type;
        set
        {
            type = value;
            NotifyPropertyListChanged();
        }
    }

    [Export] public string Description { get; set; }

    public ItemGroup Item { get; set; }

    // TODO: add properties for enemy

    private StepType type;
    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>();

        switch (Type)
        {
            case StepType.CollectItem:
            case StepType.GiveItem:
                properties.Add(new Dictionary
                {
                    { "name", nameof(Item) },
                    { "type", (int)Variant.Type.String },
                    { "usage", (int)PropertyUsageFlags.Default }
                });
                break;
        }

        return properties;
    }
}