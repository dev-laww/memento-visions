using Godot;

namespace Game.UI.Common;

[Tool]
public partial class InteractionUI : Control
{
    public string Text
    {
        get => Label.Text;
        set
        {
            Label.Text = value;
            Label.NotifyPropertyListChanged();
        }
    }

    private Label Label => GetNode<Label>("%Label");

    public void AnimateShow()
    {
        Visible = true;
        var tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(.7f, .7f), 0.2f)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine)
            .From(Vector2.Zero);

        tween.TweenProperty(this, "scale", Vector2.One * .5f, 0.1f)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);
        
        tween.TweenInterval(0.1f);
    }

    public void AnimateHide()
    {
        var tween = CreateTween();

        tween.TweenProperty(this, "scale", new Vector2(.7f, .7f), 0.2f)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);

        tween.TweenProperty(this, "scale", Vector2.Zero, 0.1f)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);

        tween.TweenCallback(Callable.From(() => Visible = false));
        tween.TweenInterval(0.1f);
    }
}
