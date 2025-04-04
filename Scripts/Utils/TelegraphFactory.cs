using Game.Common.Extensions;
using Game.Components;
using Godot;


namespace Game.Utils;

public static class TelegraphFactory
{
    private static readonly PackedScene LineTelegraphScene = ResourceLoader.Load<PackedScene>("res://Scenes/Components/Battle/Telegraph/LineTelegraph.tscn");
    private static readonly PackedScene CircleTelegraphScene = ResourceLoader.Load<PackedScene>("res://Scenes/Components/Battle/Telegraph/CircleTelegraph.tscn");

    public class LineTelegraphBuilder(TelegraphCanvas canvas, Vector2 spawnPosition)
    {
        private readonly TelegraphCanvas canvas = canvas;
        private readonly Vector2 position = spawnPosition;
        private float delay;
        private Vector2 destination;
        private float width = 16f;
        private float duration = 1.75f;

        public LineTelegraphBuilder SetDestitnation(Vector2 destination)
        {
            this.destination = destination;
            return this;
        }

        public LineTelegraphBuilder SetDelay(float delay)
        {
            this.delay = delay;
            return this;
        }

        public LineTelegraphBuilder SetDuration(float duration)
        {
            this.duration = duration;
            return this;
        }

        public LineTelegraphBuilder SetWidth(float width)
        {
            this.width = width;
            return this;
        }

        public LineTelegraph Build()
        {
            var instance = LineTelegraphScene.Instantiate<LineTelegraph>();

            canvas.AddChild(instance);
            canvas.GetTree().CreateTimer(delay).Timeout += () => instance.Start(position, destination, width);
            return instance;
        }
    }

    public class CircleTelegraphBuilder(TelegraphCanvas canvas, Vector2 spawnPosition)
    {
        private readonly TelegraphCanvas canvas = canvas;
        private readonly Vector2 position = spawnPosition;
        private float delay;
        private float radius = 16f;

        public CircleTelegraphBuilder SetDelay(float delay)
        {
            this.delay = delay;
            return this;
        }

        public CircleTelegraphBuilder SetRadius(float radius)
        {
            this.radius = radius;
            return this;
        }

        public CircleTelegraph Build()
        {
            var instance = CircleTelegraphScene.Instantiate<CircleTelegraph>();

            canvas.AddChild(instance);
            canvas.GetTree().CreateTimer(delay).Timeout += () => instance.Start(position, radius);
            return instance;
        }
    }
}

