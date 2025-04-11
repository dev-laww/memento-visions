using System.Linq;
using Game.UI.Overlays;
using Game.Common.Extensions;
using Godot;
using GodotUtilities;
using Game.Common.Utilities;
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

    public static Node CurrentScene => instance?.currentScene.GetChildren().FirstOrDefault() ??
                                       GameCamera.Instance.GetTree().CurrentScene ??
                                       GameCamera.Instance.GetTree().Root;

    public static Overlay CurrentOverlay
    {
        get => instance.currentOverlay;
        set => instance.currentOverlay = value;
    }


    public override void _Ready()
    {
        instance = this;

        if (!showStartScreen && OS.IsDebugBuild())
        {
            var lobby = resourcePreloader.InstanceSceneOrNull<Lobby>();
            currentScene.AddChild(lobby);
            return;
        }

        var startScreen = resourcePreloader.InstanceSceneOrNull<Start>();
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
            from: instance?.currentScene.GetChildren().FirstOrDefault(),
            to: instance?.currentScene
        );
    }
}