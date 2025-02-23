using System.Collections.Generic;
using System.Linq;
using DialogueManagerRuntime;
using Godot;
using Godot.Collections;

namespace Game.UI.Common;

[GlobalClass]
public partial class ResponseMenu : VBoxContainer
{
    [Export] private Control responseTemplate;
    [Export] private StringName nextAction = "";
    [Export] private bool hideFailedResponses;

    [Signal] public delegate void ResponseSelectedEventHandler(DialogueResponse response);

    public Array<DialogueResponse> Responses
    {
        get => responses;
        set
        {
            responses = value;

            foreach (var child in GetChildren())
            {
                if (child == responseTemplate) continue;

                RemoveChild(child);
                child.QueueFree();
            }

            if (responses.Count > 0)
            {
                foreach (var response in responses)
                {
                    if (hideFailedResponses && !response.IsAllowed) continue;

                    Control item = null;

                    if (IsInstanceValid(responseTemplate))
                    {
                        item = responseTemplate.Duplicate(
                            (int)DuplicateFlags.Groups |
                            (int)DuplicateFlags.Scripts |
                            (int)DuplicateFlags.Signals
                        ) as Control;

                        item?.Show();
                    }

                    item ??= new Button();
                    item.Name = $"Response{GetChildCount()}";

                    if (!response.IsAllowed)
                    {
                        item.Name += "Disallowed";
                        item.Set("disabled", true);
                    }

                    item.Set("text", response.Text);
                    item.SetMeta("response", response);

                    AddChild(item);
                }

                ConfigureFocus();
            }
        }
    }

    private Array<DialogueResponse> responses = [];


    public override void _Ready()
    {
        if (IsInstanceValid(responseTemplate)) responseTemplate.Hide();

        VisibilityChanged += OnVisibilityChanged;
    }


    private void OnVisibilityChanged()
    {
        if (IsInstanceValid(responseTemplate)) responseTemplate.Hide();
    }

    private List<Control> GetMenuItems() =>
    [
        ..GetChildren().OfType<Control>()
            .Where(child => child.Visible)
            .Where(child => !child.Name.ToString().Contains("Disallowed"))
    ];

    private void ConfigureFocus()
    {
        var items = GetMenuItems();

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            item.FocusMode = FocusModeEnum.All;

            item.FocusNeighborLeft = item.GetPath();
            item.FocusNeighborRight = item.GetPath();

            if (i == 0)
            {
                item.FocusNeighborTop = item.GetPath();
                item.FocusPrevious = item.GetPath();
            }
            else
            {
                item.FocusNeighborTop = items[i - 1].GetPath();
                item.FocusPrevious = items[i - 1].GetPath();
            }

            if (i == items.Count - 1)
            {
                item.FocusNeighborBottom = item.GetPath();
                item.FocusNext = item.GetPath();
            }
            else
            {
                item.FocusNeighborBottom = items[i + 1].GetPath();
                item.FocusNext = items[i + 1].GetPath();
            }

            if (item is Button button)
                button.Pressed += () => OnButtonPress(button);
        }
    }

    private async void OnButtonPress(Button item)
    {
        var response = item.GetMeta("response").As<DialogueResponse>();

        await ToSignal(GetTree().CreateTimer(0.1), "timeout");

        EmitSignalResponseSelected(response);
    }
}