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
        private Vector2 destination;

        public LineTelegraphBuilder SetDestitnation(Vector2 destination)
        {
            this.destination = destination;
            return this;
        }

        public LineTelegraph Build()
        {
            var instance = LineTelegraphScene.Instantiate<LineTelegraph>();
            canvas.AddChild(instance);
            instance.Start(position, destination);
            return instance;
        }
    }
}

