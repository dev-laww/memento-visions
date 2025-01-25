using Godot;
using ProjectPlugin.InspectorPlugin;

namespace ProjectPlugin;


[Tool]
public partial class ProjectPlugin : EditorPlugin
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
