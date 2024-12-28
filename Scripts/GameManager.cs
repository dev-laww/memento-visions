using System.Linq;
using Game.Globals;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game;

[Scene]
public partial class GameManager : Node
{
    [Export]
    private bool showStartScreen;

    [Node]
    private CanvasLayer userInterface;

    [Node]
    private Node currentScene;

    [Node]
    private ResourcePreloader resourcePreloader;

    private static GameManager instance;
    public static Node CurrentScene => instance.currentScene;

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
        if (what == NotificationWMCloseRequest)
        {
            // TODO: save game
            return;
        }

        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static void ChangeScene(
        string path,
        Vector2 direction = default,
        Transition transition = Transition.Fade
    ) => SceneManager.ChangeScene(
        path,
        transition: transition,
        from: instance.currentScene.GetChildren().FirstOrDefault(),
        loadInTo: instance.currentScene,
        moveDirection: direction
    );
}