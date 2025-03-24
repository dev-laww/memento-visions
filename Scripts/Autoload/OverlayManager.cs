using Game.Common;
using Game.UI.Overlays;
using Game.Utils.Extensions;
using Godot;
using GodotUtilities;

namespace Game.Autoload;

[Scene]
public partial class OverlayManager : Autoload<OverlayManager>
{
    public const string CRAFTING = "Crafting";
    public const string DEVELOPER_CONSOLE = "DeveloperConsole";
    public const string INVENTORY = "Inventory";
    public const string MENU = "Menu";
    public const string QUEST = "Quest";

    [Node] private ResourcePreloader resourcePreloader;

    private static string currentOverlayName;
    private static Overlay currentOverlay;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static void ShowOverlay(string name)
    {
        var targetOverlay = Instance.resourcePreloader.InstanceSceneOrNull<Overlay>(name);

        if (targetOverlay == null) return;

        var shouldClose = currentOverlayName == name || (name == MENU && currentOverlay != null);

        if (shouldClose)
        {
            HideOverlay();
            return;
        }

        if (currentOverlay != null) return;

        currentOverlay = targetOverlay;
        targetOverlay.TreeExiting += OnOverlayClosed;
        currentOverlayName = name;

        Instance.AddChild(currentOverlay);
        Instance.GetPlayer()?.InputManager.AddLock();
        Instance.GetViewport().SetInputAsHandled();
        Log.Debug($"Overlay {name} opended.");
    }

    public static void HideOverlay()
    {
        currentOverlay?.Close();
        OnOverlayClosed();
    }

    private static void OnOverlayClosed()
    {
        currentOverlay = null;
        currentOverlayName = null;

        Instance.GetPlayer()?.InputManager.RemoveLock();
        Log.Debug($"Overlay {currentOverlayName} closed.");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("open_inventory")) ShowOverlay(INVENTORY);
        else if (@event.IsActionPressed("menu")) ShowOverlay(MENU);
        else if (@event.IsActionPressed("open_active_quest")) ShowOverlay(QUEST);
        else if (@event.IsActionPressed("open_dev_console")) ShowOverlay(DEVELOPER_CONSOLE);
    }
}