using System.Collections.Generic;
using Godot;

namespace Game.Resources;

[Tool]
[GlobalClass, Icon("res://assets/icons/quest.svg")]
public partial class Quest : Resource
{
    [Export] public string Title;
    [Export(PropertyHint.MultilineText)] public string Description;

    [Export] private QuestStep[] steps = [];

    public IReadOnlyList<QuestStep> Steps => [.. steps];
}