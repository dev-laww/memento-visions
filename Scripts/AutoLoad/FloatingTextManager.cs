using System.Collections.Generic;
using Game.Components;
using Godot;
using GodotUtilities;

namespace Game.AutoLoad;

[Scene]
public partial class FloatingTextManager : Global<FloatingTextManager>
{
    [Node] private ResourcePreloader resourcePreloader;

    private static readonly Dictionary<Node, FloatingText> floatingTexts = [];

    public class FloatingTextSpawnAgrs
    {
        public Node Parent;
        public Vector2 Position;
        public int SpawnRadius;
        public Color? Color;
        public float? Duration;
        public string Text;
    }

    public static FloatingText SpawnDamageText(Node owner, Vector2 globalPosition, float damage)
    {
        floatingTexts.TryGetValue(owner, out var floatingText);

        var wasNull = floatingText is null;

        floatingText ??= SpawnFloatingText(new FloatingTextSpawnAgrs { Parent = owner });

        if (wasNull)
            floatingText.Finished += () => OnFloatingTextFinished(owner, floatingText);

        floatingTexts[owner] = floatingText;

        floatingText.GlobalPosition = globalPosition + (Vector2.Up * 16);
        floatingText.AddDamage(damage);
        floatingText.Start();

        return floatingText;
    }

    public static FloatingText SpawnFloatingText(FloatingTextSpawnAgrs args)
    {
        var parent = args.Parent ?? GameManager.CurrentScene ?? Instance.GetTree().CurrentScene;

        if (parent is null) return null;

        var floatingText = Instance.resourcePreloader.InstanceSceneOrNull<FloatingText>("FloatingText");

        parent.AddChild(floatingText);

        floatingText.GlobalPosition = args.Position;

        if (args.SpawnRadius > 0)
        {
            var direction = Vector2.Right.Rotated(MathUtil.RNG.RandfRange(0f, Mathf.Tau)) * MathUtil.RNG.RandfRange(0f, args.SpawnRadius);
            floatingText.GlobalPosition += direction;
        }

        floatingText.SetColor(args.Color ?? Colors.White);
        floatingText.SetText(args.Text);
        floatingText.Start(args.Duration ?? 1f);

        return floatingText;
    }

    private static void OnFloatingTextFinished(Node owner, FloatingText floatingText)
    {
        floatingTexts.Remove(owner);
        floatingText.QueueFree();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }
}