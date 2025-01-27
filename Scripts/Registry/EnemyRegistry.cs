using Game.Resources;
using Game.Utils;
using Godot;

namespace Game.Registry;

[GlobalClass]
public partial class EnemyRegistry() : Registry<Enemy, EnemyRegistry>(Constants.ENEMIES_PATH);