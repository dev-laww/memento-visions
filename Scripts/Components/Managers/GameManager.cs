using System.Linq;
using Game.Globals;
using Game.UI.Overlays;
using Game.Common.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Components.Managers;

// TODO: Rethink the GameManager
[Scene]
public partial class GameManager : Node
{
    [Export] private bool showStartScreen;

    [Node] private CanvasLayer userInterface;
    [Node] private Node currentScene;
    [Node] private ResourcePreloader resourcePreloader;

    private Overlay currentOverlay;
    private Overlay GetOverlay(string name) => userInterface.GetNodeOrNull<Overlay>(name);
    private static GameManager instance;

    private bool isDevConsoleOpen;

    public static Node CurrentScene => instance.currentScene;

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

    // TODO: Move to separate script
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("open_inventory"))
            HandleOverlay("Inventory");
        else if (@event.IsActionPressed("menu"))
            HandleOverlay("Menu");
        else if (@event.IsActionPressed("open_quests_info"))
            HandleOverlay("Quest");
        else if (@event.IsActionPressed("open_dev_console"))
            HandleOverlay("DeveloperConsole");
    }

    private void HandleOverlay(string overlayName)
    {
        var targetOverlay = GetOverlay(overlayName);

        if (targetOverlay == null) return;

        var shouldClose = currentOverlay == targetOverlay || (overlayName == "Menu" && currentOverlay != null);

        if (shouldClose)
        {
            currentOverlay.Close();
            return;
        }

        if (currentOverlay != null) return;

        currentOverlay = targetOverlay;
        currentOverlay.Open();
    }

    public static void OpenOverlay(string overlayName) => instance.HandleOverlay(overlayName);

    public static void CloseCurrentOverlay()
    {
        instance.currentOverlay?.Close();
        instance.currentOverlay = null;
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
}