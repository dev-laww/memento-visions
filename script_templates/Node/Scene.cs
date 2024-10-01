// meta-name: Base Scene
// meta-description: Predefined template for a scene script
// meta-default: true
// meta-space-indent: 4


using _BINDINGS_NAMESPACE_;
using GodotUtilities;

namespace Game;

[Scene]
public partial class _CLASS_ : _BASE_
{
    public override void _Notification(int what) {
        if (what != NotificationSceneInstantiated) return;
        
        WireNodes();
    }
}
