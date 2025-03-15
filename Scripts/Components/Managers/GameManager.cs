using System.Linq;
using Game.UI.Overlays;
using Game.Common.Extensions;
using Godot;
using GodotUtilities;
using Game.Common.Utilities;
using Game.Data;
using Game.Utils.Extensions;
using Game.Entities;
using Game.UI.Screens;
using Game.Autoload;

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

    public static Node CurrentScene => instance.currentScene.GetChildren().FirstOrDefault() ?? instance.GetTree().CurrentScene;

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
        CommandInterpreter.Register(this);
    }

    public override void _ExitTree()
    {
        CommandInterpreter.Unregister(this);
    }

    public static void ChangeScene(string path, Loading.Transition? transition = null)
    {
        SceneManager.ChangeScene(
            path,
            transition: transition,
            from: instance.currentScene.GetChildren().FirstOrDefault(),
            to: instance.currentScene
        );
    }

    [Command(Name = "spawn", Description = "Spawns an entity at the given position.")]
    private void Spawn(
        string id,
        int amount = 1,
        [CommandOption(Name = "-x", Description = "X position")]
        float x = 0,
        [CommandOption(Name = "-y", Description = "Y position")]
        float y = 0
    )
    {
        if (id.Contains("player", System.StringComparison.CurrentCultureIgnoreCase))
            throw new System.Exception("Cannot spawn player entity.");

        var entity = EntityRegistry.Get(id) ?? throw new System.Exception($"Entity with id {id} not found.");

        var position = new Vector2(x, y);
        position = position == Vector2.Zero ? this.GetPlayer()?.GlobalPosition ?? Vector2.Zero : position;

        for (var i = 0; i < amount; i++)
        {
            var instance = entity.Instantiate<Enemy>();
            instance.GlobalPosition = position + (Vector2.One * MathUtil.RNG.RandfRange(-10, 10));
            CurrentScene.AddChild(instance);
        }
    }
}