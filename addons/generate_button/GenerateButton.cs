#if TOOLS
using Game.Levels.Dungeon;
using Godot;

namespace Plugin;

[Tool]
public partial class DungeonInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object)
    {
        // Check if the object is of type Dungeon
        return @object is Dungeon;
    }

    public override void _ParseCategory(GodotObject @object, string category)
    {
        // Cast the object to Dungeon
        if (@object is not Dungeon dungeon || category != "Dungeon") return;


        // Create a new button
        var button = new Button();
        button.Text = "Generate";
        button.Pressed += () => OnGenerateButtonPressed(dungeon);

        // Add the button to the inspector
        AddCustomControl(button);
    }

    private void OnGenerateButtonPressed(Dungeon dungeon)
    {
        // Ensure the dungeon instance is valid
        // Call the Generate method on the Dungeon instance
        dungeon?.Generate();
    }
}

[Tool]
public partial class GenerateButton : EditorPlugin
{
    private DungeonInspectorPlugin _inspectorPlugin;

    public override void _EnterTree()
    {
        _inspectorPlugin = new DungeonInspectorPlugin();
        AddInspectorPlugin(_inspectorPlugin);
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(_inspectorPlugin);
    }
}
#endif