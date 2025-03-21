using System.Linq;
using Game.Common.Extensions;
using Godot;

namespace Game.Components;

[Tool]
[GlobalClass]
public partial class PassThrough : Container
{
    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.TopLeft);
        SetDeferred("size", Vector2.Zero);
        SetDeferred("custom_minimum_size", Vector2.Zero);

        if (GetParent() is not Control and not CanvasLayer)
        {
            this.GetAllChildrenOfType<Control>().ToList().ForEach(c => c.MouseFilter = MouseFilterEnum.Ignore);
            MouseFilter = MouseFilterEnum.Ignore;
            return;
        }

        GetParent().GetAllChildrenOfType<Control>().ToList().ForEach(c => c.MouseFilter = MouseFilterEnum.Ignore);
    }
}
