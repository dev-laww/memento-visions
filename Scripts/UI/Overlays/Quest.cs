using System.Linq;
using System.Text;
using Game.Autoload;
using Game.Common;
using Game.Common.Extensions;
using Game.Data;
using Godot;
using GodotUtilities;

namespace Game.UI.Overlays;

[Scene]
public partial class Quest : Overlay
{
    [Node] private Label title;
    [Node] private RichTextLabel objectives;
    [Node] private RichTextLabel description;
    [Node] private RichTextLabel rewards;
    [Node] private TextureButton closeButton;
    [Node] private Button questTitle;
    [Node] private Button activeQuestsButton;
    [Node] private VBoxContainer questTitlesContainer;
    [Node] private AudioStreamPlayer2D sfxClose;
    [Node] private AudioStreamPlayer2D sfxOpen;
    [Node] private AudioStreamPlayer2D sfxClick;

    private Godot.Collections.Dictionary<Data.Quest, Button> questButtons = [];
    private Data.Quest currentSelectedQuest;
    private string currentCategory = "active";

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        sfxOpen.Play();

        closeButton.Pressed += Close;
        questTitle.Visible = false;
        questTitle.ButtonGroup.Pressed += OnTitleButtonPressed;

        activeQuestsButton.ButtonGroup.Pressed += OnCategoryPressed;

        QuestManager.QuestAdded += _ => UpdateCategory();
        QuestManager.QuestCompleted += _ => UpdateCategory();

        Reset();
        UpdateCategory();

        if (questButtons.FirstOrDefault().Value is not { } button) return;

        button.ButtonPressed = true;
    }

    private void UpdateCurrentSelectedQuest(Data.Quest quest)
    {
        currentSelectedQuest = quest;
        title.Text = quest.Title;
        description.Text = quest.Description;
        var sb = new StringBuilder().AppendLine($"x{quest.Experience} Experience");

        foreach (var reward in quest.Items)
        {
            sb.AppendLine($"x{reward.Quantity} {reward.Item.Name}");
        }

        rewards.Text = sb.ToString();

        sb.Clear().AppendLine("Objectives:").AppendLine();

        foreach (var objective in quest.Objectives)
        {
            var color = objective.Completed ? "green" : "white";
            sb.AppendLine($"[color={color}]- {objective.Description}[/color]");

            if (objective.Enemies is not null && objective.Enemies.Length > 0)
                foreach (var killRequirement in objective.Enemies)
                {
                    var enemy = EnemyRegistry.GetAsEnemy(killRequirement.Id);
                    var enemyName = enemy?.Name ?? killRequirement.Id;
                    var quantity = objective.Completed ? killRequirement.Amount : killRequirement.Quantity;

                    sb.AppendLine(
                        $"[color={color}]  - {enemyName} x{quantity}/{killRequirement.Amount}[/color]"
                    );
                }

            if (objective.Items is null || objective.Items.Length == 0) continue;

            foreach (var itemRequirement in objective.Items)
            {
                var itemName = itemRequirement.Item.Name;
                var quantity = objective.Completed ? itemRequirement.Amount : itemRequirement.Quantity;

                sb.AppendLine(
                    $"[color={color}]  - {itemName} x{quantity}/{itemRequirement.Amount} [/color]"
                );
            }
        }

        objectives.Text = sb.ToString();
    }

    private void OnTitleButtonPressed(BaseButton button)
    {
        foreach (var (quest, questButton) in questButtons)
        {
            if (questButton != button) continue;
            sfxClick.Play();
            UpdateCurrentSelectedQuest(quest);
            break;
        }
    }

    private void OnCategoryPressed(BaseButton button)
    {
        var meta = button.GetMeta("category").AsString();

        if (string.IsNullOrEmpty(meta))
        {
            Log.Error("No category found for button.");
            return;
        }

        if (currentCategory == meta) return;

        currentCategory = meta;
        UpdateCategory();
    }

    private void UpdateCategory()
    {
        questButtons.Clear();

        foreach (var child in questTitlesContainer.GetChildren())
        {
            if (child == questTitle) continue;

            child.QueueFree();
        }

        var quests = currentCategory switch
        {
            "active" => QuestManager.ActiveQuests,
            "completed" => QuestManager.CompletedQuests,
            _ => []
        };

        foreach (var quest in quests)
        {
            var button = questTitle.Duplicate<Button>();
            questButtons[quest] = button;
            button.Text = quest.Title;
            button.Visible = true;
            questTitlesContainer.AddChild(button);
        }

        if (questButtons.Count == 0)
        {
            Reset();
            return;
        }

        if (questButtons.FirstOrDefault().Value is not { } firstTitleButton) return;

        firstTitleButton.ButtonPressed = true;
    }

    private void Reset()
    {
        title.Text = "No active quest";
        objectives.Text = string.Empty;
        description.Text = string.Empty;
        rewards.Text = string.Empty;
    }

    public override void Close()
    {
        base.Close();
        sfxClose.Play();
    }
}
