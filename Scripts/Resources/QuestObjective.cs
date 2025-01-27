using System;
using System.Linq;
using Game.Entities.Enemies;
using Godot;
using Godot.Collections;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest-step.svg")]
public partial class QuestObjective : Resource
{
    public enum ObjectiveType
    {
        Collect,
        Navigate,
        Kill,
        Deliver
    }

    [Export]
    public ObjectiveType Type
    {
        get => type;
        set
        {
            type = value;
            NotifyPropertyListChanged();
        }
    }

    [Export(PropertyHint.MultilineText)] public string Description { get; set; }

    public ItemRequirement[] Items = [];
    public KillRequirement[] Enemies = [];

    public bool Completed;

    private ObjectiveType type;

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>();

        switch (Type)
        {
            case ObjectiveType.Collect:
            case ObjectiveType.Deliver:
                properties.Add(new Dictionary
                {
                    { "name", nameof(Items) },
                    { "type", (int)Variant.Type.Array },
                    { "usage", (int)PropertyUsageFlags.Default },
                    { "hint", (int)PropertyHint.ArrayType },
                    { "hint_string", $"24/17:ItemRequirement" }
                });
                break;
            case ObjectiveType.Navigate:
                break;
            case ObjectiveType.Kill:
                properties.Add(new Dictionary
                {
                    { "name", nameof(Enemies) },
                    { "type", (int)Variant.Type.Array },
                    { "usage", (int)PropertyUsageFlags.Default },
                    { "hint", (int)PropertyHint.ArrayType },
                    { "hint_string", $"24/17:KillRequirement" }
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return properties;
    }

    public void UpdateItemProgress(ItemGroup group)
    {
        ValidateType(ObjectiveType.Collect, ObjectiveType.Deliver);
        if (Completed) return;

        UpdateRequirements(
            Items,
            req => req.Item.Id,
            group.Item.Id,
            group.Quantity
        );

        Completed = CheckCompletion(Items);
    }

    public void UpdateKillProgress(Enemy enemy)
    {
        ValidateType(ObjectiveType.Kill);
        if (Completed) return;

        UpdateRequirements(
            Enemies,
            req => req.Id,
            enemy.Id,
            1
        );

        Completed = CheckCompletion(Enemies);
    }

    private void ValidateType(params ObjectiveType[] allowedTypes)
    {
        if (allowedTypes.Contains(Type)) return;

        GD.PushWarning($"{this} does not require this type of objective. Use the appropriate method instead.");
        throw new InvalidOperationException();
    }

    private static bool CheckCompletion<T>(T[] requirements) where T : class => requirements.All(r =>
        (int)r.GetType().GetProperty("Quantity")?.GetValue(r)! >=
        (int)r.GetType().GetProperty("Amount")?.GetValue(r)!
    );

    private static void UpdateRequirements<T>(
        T[] requirements,
        Func<T, string> getId,
        string targetId,
        int amount
    )
    {
        foreach (var req in requirements)
        {
            if (getId(req) != targetId) continue;
            var quantity = (int)req.GetType().GetProperty("Quantity")?.GetValue(req)!;
            var amountProp = (int)req.GetType().GetProperty("Amount")?.GetValue(req)!;

            req.GetType().GetProperty("Quantity")?.SetValue(
                req,
                Mathf.Min(amountProp, quantity + amount)
            );
        }
    }

    public override string ToString() => $"<QuestObjective ({Type} {GetHashCode()})>";
}