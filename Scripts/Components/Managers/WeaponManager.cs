using Godot;
using GodotUtilities;

namespace Game.Components.Managers;

// TODO: Implement me
[Scene]
public partial class WeaponManager : Node2D
{
	public override void _Notification(int what) {
		if (what != NotificationSceneInstantiated) return;
		
		WireNodes();
	}
}

