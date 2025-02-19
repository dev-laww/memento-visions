using System.Linq;
using Game.Autoload;
using Game.UI.Overlays;
using Game.Common.Extensions;
using Godot;
using GodotUtilities;
using Game.Common.Utilities;
using Game.Data;
using Game.Exceptions;
using Game.Utils.Extensions;

namespace Game.Components;

// TODO: Rethink the GameManager
[Scene]
public partial class GameManager : Node
{
    [Export] private bool showStartScreen;

    [Node] private Node currentScene;
    [Node] private ResourcePreloader resourcePreloader;

    private Overlay currentOverlay;

    private static GameManager instance;

    private bool isDevConsoleOpen;

    public static Node CurrentScene => instance.currentScene.GetChildren().FirstOrDefault();

    public static Overlay CurrentOverlay
    {
        get => instance.currentOverlay;
        set => instance.currentOverlay = value;
    }


    public override void _Ready()
    {
        instance = this;
        if (!showStartScreen && OS.IsDebugBuild()) return;

        currentScene.GetChildren().ToList().ForEach(c => c.QueueFree());
        var startScreen = resourcePreloader.GetResource<PackedScene>("StartScreen").Instantiate();
        currentScene.AddChild(startScreen);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _EnterTree()
    {
        CommandInterpreter.Register("spawn", Spawn, "Spawns an entity at the given position. Usage: spawn [entity_id] [x] [y]");
    }

    public override void _ExitTree()
    {
        CommandInterpreter.Unregister("spawn");
    }

    public static void ChangeScene(
        string path,
        Vector2 direction = default,
        Transition transition = Transition.Fade
    )
    {
        SceneManager.ChangeScene(
            path,
            transition: transition,
            from: instance.currentScene.GetChildren().FirstOrDefault(),
            loadInTo: instance.currentScene,
            moveDirection: direction
        );
    }

    private void Spawn(string id, float x = 0, float y = 0)
    {
        if (id.ToLower().Contains("player"))
            throw new CommandException("Cannot spawn player entity.");

        var entity = EntityRegistry.GetAsEntity(id) ?? throw new CommandException($"Entity with id {id} not found.");

        var position = new Vector2(x, y);
        position = position == Vector2.Zero ? this.GetPlayer()?.GlobalPosition ?? Vector2.Zero : position;

        CurrentScene.AddChild(entity);
        entity.GlobalPosition = position;
    }
}