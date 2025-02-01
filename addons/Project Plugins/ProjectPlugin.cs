#if TOOLS

using Godot;
using ProjectPlugin.InspectorPlugin;

namespace ProjectPlugin;


[Tool]
public partial class ProjectPlugin : EditorPlugin
{
    private DungeonInspectorPlugin _inspectorPlugin;
    private IdInspectorPlugin _idInspectorPlugin;

    public override void _EnterTree()
    {
        _inspectorPlugin = new DungeonInspectorPlugin();
        _idInspectorPlugin = new IdInspectorPlugin();

        AddInspectorPlugin(_inspectorPlugin);
        AddInspectorPlugin(_idInspectorPlugin);
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(_inspectorPlugin);
        RemoveInspectorPlugin(_idInspectorPlugin);
    }
}

#endif
