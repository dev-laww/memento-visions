#if TOOLS

using Game.Data;
using Game.Entities;
using Godot;

namespace ProjectPlugin.InspectorPlugin;

public partial class IdInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object) => @object is Quest or Item or Entity;

    public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        if (name != "Id") return false;

        var hbox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var labelHbox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var editor = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        var label = new Label { Text = "Id", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

        labelHbox.AddChild(label);
        hbox.AddChild(labelHbox);
        hbox.AddChild(editor);

        var lineEdit = new LineEdit
        {
            Text = @object.Get(name).ToString(),
            Editable = @object.Get(name).ToString() == string.Empty,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        var margin = new MarginContainer();

        margin.AddThemeConstantOverride("margin_left", 5);

        Button createButton()
        {
            var button = new Button { Text = "Reset" };
            button.Pressed += () =>
            {
                @object.Set(name, "");
                lineEdit.Text = "";
                lineEdit.Editable = true;
                lineEdit.GrabFocus();

                button.QueueFree();

                var obj = @object as Resource;
                ResourceSaver.Save(obj, obj.ResourcePath);
            };

            return button;
        }

        void SubmitText(string text)
        {
            if (text == string.Empty)
            {
                GD.PrintErr("Id cannot be empty.");
                return;
            }

            var existing = ItemRegistry.Get(text) as Resource ?? QuestRegistry.Get(text);
            var obj = @object as Resource;

            if (existing != null && existing.ResourcePath != obj.ResourcePath)
            {
                GD.PrintErr($"Id '{text}' already exists. Please choose a different id. {existing.ResourcePath}");
                return;
            }

            @object.Set(name, text);
            lineEdit.Text = text;
            lineEdit.Editable = false;

            if (labelHbox.GetChildCount() != 1) return;

            labelHbox.AddChild(createButton());
        }

        lineEdit.TextSubmitted += SubmitText;
        lineEdit.TextChanged += text => @object.Set(name, text);
        lineEdit.FocusExited += () =>
        {
            if (!lineEdit.Editable) return;

            SubmitText(lineEdit.Text);
        };


        if (!lineEdit.Editable && labelHbox.GetChildCount() == 1)
            labelHbox.AddChild(createButton());

        editor.AddChild(lineEdit);

        margin.AddChild(hbox);

        AddCustomControl(margin);

        return true;
    }
}

#endif