using Game.UI.Overlays;
using Game.UI.Screens;
using Godot;
using GodotUtilities;


namespace Game.Utils;

public static class OverlayFactory
{
    private static readonly PackedScene textLoadingScene = ResourceLoader.Load<PackedScene>("res://Scenes/UI/Screens/TextLoading.tscn");
    private static readonly PackedScene centerTextScene = ResourceLoader.Load<PackedScene>("res://Scenes/UI/Overlays/CenterText.tscn");

    public class TextLoadingBuilder(SceneTree tree)
    {
        private string text = "Loading...";
        private float duration;

        public TextLoadingBuilder SetText(string text)
        {
            this.text = text;
            return this;
        }

        public TextLoadingBuilder SetDuration(float duration)
        {
            this.duration = duration;
            return this;
        }

        public TextLoading Build()
        {
            var instance = textLoadingScene.Instantiate<TextLoading>();
            instance.Text = text;

            tree.Root.AddChildDeferred(instance);

            if (duration > 0)
                tree.CreateTimer(duration).Timeout += instance.QueueFree;

            return instance;
        }
    }

    public class CenterTextBuilder(SceneTree tree)
    {
        private string text = ":DD";
        private float duration;

        public CenterTextBuilder SetText(string text)
        {
            this.text = text;
            return this;
        }

        public CenterTextBuilder SetDuration(float duration)
        {
            this.duration = duration;
            return this;
        }

        public CenterText Build()
        {
            var instance = centerTextScene.Instantiate<CenterText>();

            tree.Root.AddChildDeferred(instance);

            instance.Text = text;
            instance.Start();

            if (duration > 0)
            {
                tree.CreateTimer(duration).Timeout += () =>
                {
                    instance.AnimationFinished += instance.QueueFree;
                    instance.End();
                };
            }

            return instance;
        }
    }
}
