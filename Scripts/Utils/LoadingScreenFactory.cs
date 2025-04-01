using Game.UI.Screens;
using Godot;
using GodotUtilities;


namespace Game.Utils;

public static class LoadingScreenFactory
{
    private static readonly PackedScene textLoadingScene = ResourceLoader.Load<PackedScene>("res://Scenes/UI/Screens/TextLoading.tscn");
    public class TextLoadingBuilder(SceneTree tree)
    {
        private string text = "Loading...";

        public TextLoadingBuilder SetText(string text)
        {
            this.text = text;
            return this;
        }

        public TextLoading Build()
        {
            var instance = textLoadingScene.Instantiate<TextLoading>();
            instance.Text = text;

            tree.Root.AddChildDeferred(instance);

            return instance;
        }
    }
}
