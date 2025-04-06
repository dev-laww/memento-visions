#if TOOLS

using Godot;
using Game.World;

namespace ProjectPlugin.InspectorPlugin;

[Tool]
public partial class DungeonInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object) => @object is Dungeon or WaveFunctionCollapse;

    public override void _ParseCategory(GodotObject @object, string category)
    {
        // Create a new generate
        var generate = new Button { Text = "Generate" };

        // Add the generate to the inspector
        AddCustomControl(generate);

        // Cast the object to Dungeon
        if (@object is Dungeon dungeon)
        {
            if (category != "Dungeon")
            {
                return;
            }

            generate.Pressed += dungeon.Generate;
        }
        else if (@object is WaveFunctionCollapse wave)
        {
            if (category != "WaveFunctionCollapse")
            {
                return;
            }

            var clear = new Button { Text = "Clear" };

            AddCustomControl(clear);

            generate.Pressed += wave.Generate;
            clear.Pressed += wave.Clear;
        }
    }
}
#endif