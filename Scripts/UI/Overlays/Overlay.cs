using Game.Components.Managers;
using Game.Utils.Extensions;
using Godot;

namespace Game.UI.Overlays;

// TODO: animate open/close
public abstract partial class Overlay : Control
{
    public void Toggle()
    {
        if (Visible)
            Close();
        else
            Open();
    }

    public virtual void Close()
    {
        this.GetPlayer()?.InputManager.RemoveLock();

        SetProcessInput(false);
        SetProcessUnhandledInput(false);
        MouseFilter = MouseFilterEnum.Pass;

        if (GameManager.CurrentOverlay == this) GameManager.CurrentOverlay = null;

        Hide();
    }

    public virtual void Open()
    {
        this.GetPlayer()?.InputManager.AddLock();

        Show();
        SetProcessInput(true);
        SetProcessUnhandledInput(true);

        MouseFilter = MouseFilterEnum.Stop;
    }
}