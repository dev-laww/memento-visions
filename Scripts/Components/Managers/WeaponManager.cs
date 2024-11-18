using Game.Resources;
using Godot;
using GodotUtilities;

namespace Game.Components.Managers;

// TODO: Implement me
[Scene]
public partial class WeaponManager : Node2D
{
    [Signal]
    public delegate void WeaponChangedEventHandler();

    public Weapon CurrentWeapon { get; private set; }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public void ChangeWeapon(string weapon)
    {
        EmitSignal(SignalName.WeaponChanged);
        
        // load json then get resource path
    }
}