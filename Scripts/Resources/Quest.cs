using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest.svg")]
public partial class Quest : Resource
{
    [Export] public string Id { get; private set; } = Guid.NewGuid().ToString();
    [Export] public string Title;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] private QuestObjective[] objectives = [];
    [Export] public bool Ordered;

    public List<QuestObjective> Objectives => [.. objectives];

    public override void _ValidateProperty(Dictionary property)
    {
        base._ValidateProperty(property);

        if (property["name"].AsString() != PropertyName.Id) return;

        var usage = property["usage"].As<PropertyUsageFlags>();

        usage |= PropertyUsageFlags.ReadOnly;

        property["usage"] = (int)usage;
    }
}