using Godot;
using System.Linq;
using Game.Autoload;
using GodotUtilities;
using Game.Components;
using Game.Utils.Extensions;
using QuestResource = Game.Data.Quest;
using System;

namespace Game.UI.Overlays;

[Scene]
public partial class Quest : Overlay
{
    [Node] public Tree QuestTree;
    [Node] public RichTextLabel Description;
    [Node] public Label Title;
    [Node] public RichTextLabel Objectives;
    [Node] public Label Reward;
    [Node] public TextureButton CloseButton;

    private TreeItem TreeRoot;

    public override void _Notification(int what)
    {
        if (what != NotificationSceneInstantiated) return;
        WireNodes();
    }

    public override void _Ready()
    {
        base._Ready();
        CloseButton.Pressed += Close;
        QuestManager.QuestAdded += OnQuestAdded;
        QuestManager.QuestUpdated += OnQuestUpdated;
        QuestManager.QuestCompleted += OnQuestCompleted;
        QuestTree.ItemSelected += OnItemSelected;

        InitializeTree();
        if (TreeRoot != null && TreeRoot.GetChildCount() > 0)
        {
            var firstItem = TreeRoot.GetChild(0);
            QuestTree.SetSelected(firstItem, 0);
            OnItemSelected(); // Manually trigger details update
        }
    }

    private void OnItemSelected()
    {
        var item = QuestTree.GetSelected();
        if (item == null || !IsInstanceValid(item)) return;

        var metadata = item.GetMetadata(0);
        var quest = metadata.As<QuestResource>();
        if (quest == null) return;

        UpdateQuestDetails(quest);
    }


    public void InitializeTree()
    {
        QuestTree.Clear();
        QuestTree.SetColumnTitle(0, "Quests");
        TreeRoot = QuestTree.CreateItem();
        foreach (var item in QuestManager.Quests)
        {
            var treeItem = QuestTree.CreateItem(TreeRoot);
            treeItem.SetText(0, item.Title);
            treeItem.SetMetadata(0, item);
        }
    }


    private void OnQuestUpdated(QuestResource quest)
    {
        foreach (var item in TreeRoot.GetChildren())
        {
            if (item == null || !IsInstanceValid(item)) continue;
            if ((QuestResource)item.GetMetadata(0) != quest) continue;

            item.SetText(0, quest.Title);

            if (QuestTree.GetSelected() == item)
            {
                UpdateQuestDetails(quest);
            }

            break;
        }
    }

    private void OnQuestAdded(QuestResource quest)
    {
        if (TreeRoot == null) return;

        var item = QuestTree.CreateItem(TreeRoot);
        item.SetText(0, quest.Title);
        item.SetMetadata(0, quest);
    }


    private void OnQuestCompleted(QuestResource quest)
    {
        foreach (var item in TreeRoot.GetChildren())
        {
            if (item == null || !IsInstanceValid(item)) continue;
            if ((QuestResource)item.GetMetadata(0) != quest) continue;

            TreeRoot.RemoveChild(item);
            break;
        }
    }


    private void UpdateQuestDetails(QuestResource quest)
    {
        Title.Text = quest.Title;
        Description.Text = quest.Description;
        Objectives.Text = "OBJECTIVE:\n" + string.Join("\n", quest.Objectives.Select(objective =>
        {
            var color = objective.Completed ? "green" : "white";
            return $"[color={color}]{objective.Description}[/color]";
        }));
        Reward.Text =
            $"Experience: {quest.Experience}\nItems: {string.Join(", ", quest.Items.Select(item => item.ResourceName))}";
    }


    public override void _ExitTree()
    {
        QuestManager.QuestAdded -= OnQuestAdded;
        QuestManager.QuestUpdated -= OnQuestUpdated;
        QuestManager.QuestCompleted -= OnQuestCompleted;
        QuestTree.ItemSelected -= OnItemSelected;
        CloseButton.Pressed -= Close;
    }
}