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
        Size = Vector2.Zero;
        CustomMinimumSize = Vector2.Zero;
        NotifyPropertyListChanged();

        if (GetParent() is not Control parent)
        {
            this.GetAllChildrenOfType<Control>().ToList().ForEach(c => c.MouseFilter = MouseFilterEnum.Ignore);
            MouseFilter = MouseFilterEnum.Ignore;
            return;
        }

        parent.GetAllChildrenOfType<Control>().ToList().ForEach(c => c.MouseFilter = MouseFilterEnum.Ignore);
    }
}
