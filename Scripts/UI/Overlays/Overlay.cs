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
        this.GetPlayer()?.SetProcessInput(true);
        SetProcessInput(false);
        SetProcessUnhandledInput(false);

        if (GameManager.CurrentOverlay == this) GameManager.CurrentOverlay = null;

        Hide();
    }

    public virtual void Open()
    {
        this.GetPlayer()?.SetProcessInput(false);
        Show();
        SetProcessInput(true);
        SetProcessUnhandledInput(true);
    }
}