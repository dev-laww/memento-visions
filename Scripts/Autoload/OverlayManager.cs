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
    public const string MODE_SELECT = "ModeSelect";
    public const string CONCOCT = "Concoct";
    public const string CHARACTER_DETAILS = "CharacterDetails";
    public const string ENEMY_GLOSSARY = "EnemyGlossary";
    public const string CONTROL_GUIDE = "ControlGuide";

    [Node] private ResourcePreloader resourcePreloader;

    private static string currentOverlayName;
    public static Overlay CurrentOverlay { get; private set; }
    public static bool HasOpenOverlay => CurrentOverlay != null;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public static Overlay ShowOverlay(string name)
    {
        var targetOverlay = Instance.resourcePreloader.InstanceSceneOrNull<Overlay>(name);

        if (targetOverlay == null) return null;

        var shouldClose = currentOverlayName == name || (name == MENU && CurrentOverlay != null);

        if (shouldClose)
        {
            HideOverlay();
            return null;
        }

        if (CurrentOverlay != null) return null;

        CurrentOverlay = targetOverlay;
        targetOverlay.TreeExiting += OnOverlayClosed;
        currentOverlayName = name;

        Instance.AddChild(CurrentOverlay);
        Instance.GetPlayer()?.InputManager.AddLock();
        Instance.GetViewport().SetInputAsHandled();
        Log.Debug($"Overlay {name} opended.");

        return CurrentOverlay;
    }

    public static void HideOverlay()
    {
        CurrentOverlay?.Close();
        OnOverlayClosed();
    }

    private static void OnOverlayClosed()
    {
        CurrentOverlay = null;
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
        else if (@event.IsActionPressed("open_character_details")) ShowOverlay(CHARACTER_DETAILS);
        else if (@event.IsActionPressed("control_guide")) ShowOverlay(CONTROL_GUIDE);
    }

}