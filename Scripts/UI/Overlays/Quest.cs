using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Autoload;
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
    [Node] private VBoxContainer questTitlesContainer;

    private Godot.Collections.Dictionary<Data.Quest, Button> questButtons = [];
    private Data.Quest currentSelectedQuest;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;

        WireNodes();
    }

    public override void _Ready()
    {
        closeButton.Pressed += Close;
        questTitle.Visible = false;

        QuestManager.QuestAdded += OnQuestAdded;
        QuestManager.QuestCompleted += OnQuestCompleted;
        QuestManager.QuestUpdated += OnQuestUpdated;

        questTitle.ButtonGroup.Pressed += OnButtonPressed;

        QuestManager.Quests.ToList().ForEach(OnQuestAdded);
        Reset();

        if (questButtons.FirstOrDefault().Value is not { } button) return;

        button.ButtonPressed = true;
    }

    public override void _ExitTree()
    {
        QuestManager.QuestAdded -= OnQuestAdded;
        QuestManager.QuestCompleted -= OnQuestCompleted;
        QuestManager.QuestUpdated -= OnQuestUpdated;
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
            sb.AppendLine($"- [color={color}]{objective.Description}[/color]");

            if (objective.Enemies is not null && objective.Enemies.Length > 0)
                foreach (var killRequirement in objective.Enemies)
                {
                    var enemy = EnemyRegistry.GetAsEnemy(killRequirement.Id);
                    sb.AppendLine(
                        $"  - {enemy.EnemyName} [color={color}]x{killRequirement.Quantity}/{killRequirement.Amount}[/color]"
                    );
                }

            if (objective.Items is null || objective.Items.Length == 0) continue;

            foreach (var itemRequirement in objective.Items)
            {
                var itemName = itemRequirement.Item.Name;
                sb.AppendLine(
                    $"  - {itemName} [color={color}]x{itemRequirement.Quantity}/{itemRequirement.Amount} [/color]"
                );
            }
        }

        objectives.Text = sb.ToString();
    }

    private void OnQuestAdded(Data.Quest quest)
    {
        var button = questTitle.Duplicate<Button>();
        questButtons[quest] = button;
        button.Text = quest.Title;
        button.Visible = true;
        questTitlesContainer.AddChild(button);
    }

    private void OnQuestUpdated(Data.Quest quest)
    {
        if (!questButtons.ContainsKey(quest) || currentSelectedQuest != quest) return;
        UpdateCurrentSelectedQuest(quest);
    }

    private void OnQuestCompleted(Data.Quest quest)
    {
        if (!questButtons.Remove(quest, out var button)) return;

        button.QueueFree();
    }

    private void OnButtonPressed(BaseButton button)
    {
        foreach (var (quest, questButton) in questButtons)
        {
            if (questButton != button) continue;

            UpdateCurrentSelectedQuest(quest);
            break;
        }
    }

    private void Reset()
    {
        title.Text = "No active quest";
        objectives.Text = string.Empty;
        description.Text = string.Empty;
        rewards.Text = string.Empty;
    }
}