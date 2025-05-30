using Godot;
using Godot.Collections;
using DialogueManagerRuntime;
using GodotUtilities;
using System.Threading.Tasks;
using Game.Utils.Extensions;
using Game.Components;

namespace Game.UI.Common;

[Scene]
public partial class Balloon : CanvasLayer
{
    [Export] public string NextAction = "ui_accept";
    [Export] public string SkipAction = "ui_cancel";

    [Node] private Control balloon;
    [Node] private RichTextLabel characterLabel;
    [Node] private RichTextLabel dialogueLabel;
    [Node] private ResponseMenu responsesMenu;
    [Node] private Timer mutationCooldown = new();

    private Node CurrentScene => GameManager.CurrentScene;

    private Resource resource;
    private Array<Variant> temporaryGameStates = [];
    private bool isWaitingForInput;
    private bool willHideBalloon;

    private DialogueLine dialogueLine;

    private DialogueLine DialogueLine
    {
        get => dialogueLine;
        set
        {
            if (value == null)
            {
                QueueFree();
                this.GetPlayer()?.InputManager.RemoveLock();
                return;
            }

            dialogueLine = value;
            ApplyDialogueLine();
        }
    }

    public override void _Ready()
    {
        balloon.Hide();
        DialogueManager.Mutated += OnMutated;
    }

    public override void _ExitTree() => DialogueManager.Mutated -= OnMutated;

    public override void _UnhandledInput(InputEvent @event) => GetViewport().SetInputAsHandled();

    public override async void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated)
        {
            WireNodes();
            return;
        }

        // Detect a change of locale and update the current dialogue line to show the new language
        if (what == NotificationTranslationChanged && IsInstanceValid(dialogueLabel) && DialogueLine != null)
        {
            var visibleRatio = dialogueLabel.VisibleRatio;
            DialogueLine = await Next(DialogueLine.Id);
            
        }
    }

    public async void Start(Resource dialogueResource, string title, Array<Variant> extraGameStates = null)
    {
        temporaryGameStates = new Array<Variant> { this } + (extraGameStates ?? []);
        isWaitingForInput = false;
        resource = dialogueResource;

        DialogueLine = await Next(title);
        this.GetPlayer()?.InputManager.AddLock();
    }

    public async Task<DialogueLine> Next(string nextId)
    {
        DialogueLine = await DialogueManager.GetNextDialogueLine(resource, nextId, temporaryGameStates);
        return DialogueLine;
    }

    #region Helpers

    private async void ApplyDialogueLine()
    {
        mutationCooldown.Stop();

        isWaitingForInput = false;
        balloon.FocusMode = Control.FocusModeEnum.All;
        balloon.GrabFocus();

        // Set up the character name
        characterLabel.Visible = !string.IsNullOrEmpty(dialogueLine.Character);
        characterLabel.Text = Tr(dialogueLine.Character, "dialogue");

        // Set up the dialogue
        dialogueLabel.Hide();
        dialogueLabel.Set("dialogue_line", dialogueLine);

        // Set up the responses
        responsesMenu.Hide();
        responsesMenu.Responses = dialogueLine.Responses;

        // Type out the text
        balloon.Show();
        willHideBalloon = false;
        dialogueLabel.Show();

        if (!string.IsNullOrEmpty(dialogueLine.Text))
        {
            dialogueLabel.Call("type_out");
            await ToSignal(dialogueLabel, "finished_typing");
        }

        // Wait for input
        if (dialogueLine.Responses.Count > 0)
        {
            balloon.FocusMode = Control.FocusModeEnum.None;
            responsesMenu.Show();
        }
        else if (!string.IsNullOrEmpty(dialogueLine.Time))
        {
            if (!float.TryParse(dialogueLine.Time, out var time))
            {
                time = dialogueLine.Text.Length * 0.02f;
            }

            await ToSignal(GetTree().CreateTimer(time), "timeout");
            await Next(dialogueLine.NextId);
        }
        else
        {
            isWaitingForInput = true;
            balloon.FocusMode = Control.FocusModeEnum.All;
            balloon.GrabFocus();
        }
    }

    #endregion


    #region signals

    private async void OnGuiInput(InputEvent @event)
    {
        if ((bool)dialogueLabel.Get("is_typing"))
        {
            var mouseWasClicked = @event is InputEventMouseButton && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left && @event.IsPressed();
            var skipButtonWasPressed = @event.IsActionPressed(SkipAction);

            if (!mouseWasClicked && !skipButtonWasPressed) return;

            GetViewport().SetInputAsHandled();
        }

        if (!isWaitingForInput) return;
        if (dialogueLine.Responses.Count > 0) return;

        GetViewport().SetInputAsHandled();

        if (@event is InputEventMouseButton && @event.IsPressed() && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left)
        {
            await Next(dialogueLine.NextId);
        }
        else if (@event.IsActionPressed(NextAction) && GetViewport().GuiGetFocusOwner() == balloon)
        {
            await Next(dialogueLine.NextId);
        }
    }

    private async void OnResponseSelected(DialogueResponse response) => await Next(response.NextId);

    private void OnMutationCooldownTimeout()
    {
        if (willHideBalloon)
        {
            willHideBalloon = false;
            balloon.Hide();
        }
    }

    private void OnMutated(Dictionary _mutation)
    {
        isWaitingForInput = false;
        willHideBalloon = true;
        mutationCooldown.Start(0.1f);
    }

    #endregion
}