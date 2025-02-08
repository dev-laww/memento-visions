using System.Collections.Generic;
using Game.Generation;
using Godot;
using GodotUtilities.Logic;

namespace Game.Levels;


public partial class WaveFunctionCollapse : Node2D
{
    [Export] private WaveFuncitonCollapseSettings settings;

    private LootTable<WaveFunctionCollapseEntry> table = new();
    private Grid<string> grid;

    public override void _Ready()
    {
        foreach (var entry in settings.Entries)
            table.AddItem(entry, entry.Weight);

        grid = new Grid<string>(settings.gridSize, Vector2I.Zero);
    }

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (settings is null)
            return ["Settings is not set"];

        if (settings.Entries.Length == 0)
            warnings.Add("Entries is empty");

        return [.. warnings];
    }
}