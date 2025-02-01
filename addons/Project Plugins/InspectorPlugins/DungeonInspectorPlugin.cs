#if TOOLS

using Godot;
using Game.Levels.Dungeon;

namespace ProjectPlugin.InspectorPlugin;

[Tool]
public partial class DungeonInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object) => @object is Dungeon;

    public override void _ParseCategory(GodotObject @object, string category)
    {
        // Cast the object to Dungeon
        if (@object is not Dungeon dungeon || category != "Dungeon") return;

        // Create a new button
        var button = new Button { Text = "Generate" };
        button.Pressed += () => OnGenerateButtonPressed(dungeon);

        // Add the button to the inspector
        AddCustomControl(button);
    }
    private static void OnGenerateButtonPressed(Dungeon dungeon) => dungeon.Generate();
}

# endif